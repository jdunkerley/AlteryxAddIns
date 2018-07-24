import AlteryxPythonSDK as Sdk
import xml.etree.ElementTree as Et

class AyxPlugin:
    """
    Implements the plugin interface methods, to be utilized by the Alteryx engine to communicate with a plugin.
    Prefixed with "pi", the Alteryx engine will expect the below five interface methods to be defined.
    """

    def __init__(self, n_tool_id: int, alteryx_engine: object, output_anchor_mgr: object):
        """
        Constructor is called whenever the Alteryx engine wants to instantiate an instance of this plugin.
        :param n_tool_id: The assigned unique identification for a tool instance.
        :param alteryx_engine: Provides an interface into the Alteryx engine.
        :param output_anchor_mgr: A helper that wraps the outgoing connections for a plugin.
        """

        # Default properties
        self.n_tool_id = n_tool_id
        self.alteryx_engine = alteryx_engine
        self.output_anchor_mgr = output_anchor_mgr

        # Custom properties
        self.is_initialized = True

        self.single_input = None
        self.grouping_fields = None
        self.xml_sort_info = None

        self.output_anchor = None
        self.output_field = None
        self.output_field_name = None
        self.output_field_type = None
        self.output_field_size = None
        self.output_field_last = False

        self.starting_value = None
        self.current_value = None
        self.current_group = None
        self.grouping_fields_objects = None

    def pi_init(self, str_xml: str):
        """
        Handles configuration based on the GUI.
        Called when the Alteryx engine is ready to provide the tool configuration from the GUI.
        :param str_xml: The raw XML from the GUI.
        """

        # Getting the dataName data property from the Gui.html
        self.output_field_name = Et.fromstring(str_xml).find('FieldName').text if 'FieldName' in str_xml else None
        if self.output_field_name is None:
            self.display_error_msg('Field name cannot be empty. Please enter a field name.')
        elif len(self.output_field_name) > 255:
            self.display_error_msg('Field name cannot be greater than 255 characters.')

        self.output_field_last = Et.fromstring(str_xml).find('LastColumn').text if 'LastColumn' in str_xml else 'False' == 'True'

        # Assigning the appropriate Alteryx field type.
        field_type = Et.fromstring(str_xml).find('FieldType').text if 'FieldType' in str_xml else None
        if field_type == 'Byte':
            self.output_field_type = Sdk.FieldType.byte
        elif field_type == 'Int16':
            self.output_field_type = Sdk.FieldType.int16
        elif field_type == 'Int32':
            self.output_field_type = Sdk.FieldType.int32
        elif field_type == 'Int64':
            self.output_field_type = Sdk.FieldType.int64
        elif field_type == 'String':
            self.output_field_type = Sdk.FieldType.string
            self.output_field_size = int(Et.fromstring(str_xml).find('StringSize').text) if 'StringSize' in str_xml else 1
        elif field_type == 'WString':
            self.output_field_type = Sdk.FieldType.wstring
            self.output_field_size = int(Et.fromstring(str_xml).find('StringSize').text) if 'StringSize' in str_xml else 1
        elif field_type == 'V_String':
            self.output_field_type = Sdk.FieldType.v_string
            self.output_field_size = int(Et.fromstring(str_xml).find('StringSize').text) if 'StringSize' in str_xml else 1
        elif field_type == 'V_WString':
            self.output_field_type = Sdk.FieldType.v_wstring
            self.output_field_size = int(Et.fromstring(str_xml).find('StringSize').text) if 'StringSize' in str_xml else 1
        else:
            self.display_error_msg('Field type must be selected.')

        # Initial Value For ID
        self.starting_value = int(Et.fromstring(str_xml).find('StartingValue').text) if 'StartingValue' in str_xml else 1
        self.current_value = self.starting_value - 1

        # Build Sorting String
        self.grouping_fields = Et.fromstring(str_xml).find('GroupingFields').text.split(',') if 'GroupingFields' in str_xml else []
        sorting_fields = Et.fromstring(str_xml).find('SortingFields').text.split(',') if 'SortingFields' in str_xml else []
        descending_fields = Et.fromstring(str_xml).find('DescendingFields').text.split(',') if 'DescendingFields' in str_xml else []
        self.xml_sort_info = ''
        for grouping_field in self.grouping_fields:
            if grouping_field != '""':
                self.xml_sort_info += '<Field field=' + grouping_field + ' order="' + ('Desc' if grouping_field in descending_fields else 'Asc') + '" />\n'
        for grouping_field in sorting_fields:
            if not grouping_field in self.grouping_fields and grouping_field != '""':
                self.xml_sort_info += '<Field field=' + grouping_field + ' order="' + ('Desc' if grouping_field in descending_fields else 'Asc') + '" />\n'
        self.xml_sort_info = ('<SortInfo>\n<GroupByFields>\n' + self.xml_sort_info + '</GroupByFields>\n</SortInfo>\n') if self.xml_sort_info else ''
        self.alteryx_engine.output_message(self.n_tool_id, Sdk.EngineMessageType.info, self.xml_sort_info)

        # Getting the output anchor from Config.xml by the output connection name
        self.output_anchor = self.output_anchor_mgr.get_output_anchor('Output')

    def pi_add_incoming_connection(self, str_type: str, str_name: str) -> object:
        """
        The IncomingInterface objects are instantiated here, one object per incoming connection.
        Called when the Alteryx engine is attempting to add an incoming data connection.
        :param str_type: The name of the input connection anchor, defined in the Config.xml file.
        :param str_name: The name of the wire, defined by the workflow author.
        :return: The IncomingInterface object(s).
        """
        self.alteryx_engine.pre_sort(str_type, str_name, self.xml_sort_info)
        self.single_input = IncomingInterface(self)
        return self.single_input

    def pi_add_outgoing_connection(self, str_name: str) -> bool:
        """
        Called when the Alteryx engine is attempting to add an outgoing data connection.
        :param str_name: The name of the output connection anchor, defined in the Config.xml file.
        :return: True signifies that the connection is accepted.
        """
        return True

    def pi_push_all_records(self, n_record_limit: int) -> bool:
        """
        Handles generating a new field for no incoming connections.
        Called when a tool has no incoming data connection.
        :param n_record_limit: Set it to <0 for no limit, 0 for no records, and >0 to specify the number of records.
        :return: False if there's an error with the field name, otherwise True.
        """
        return False

    def pi_close(self, b_has_errors: bool):
        """
        Called after all records have been processed.
        :param b_has_errors: Set to true to not do the final processing.
        """
        self.output_anchor.assert_close()

    def display_error_msg(self, msg_string: str):
        """
        A non-interface method, that is responsible for displaying the relevant error message in Designer.
        :param msg_string: The custom error message.
        """
        self.is_initialized = False

class IncomingInterface:
    """
    This class is returned by pi_add_incoming_connection, and it implements the incoming interface methods, to be\
    utilized by the Alteryx engine to communicate with a plugin when processing an incoming connection.
    Prefixed with "ii", the Alteryx engine will expect the below four interface methods to be defined.
    """

    def __init__(self, parent: object):
        """
        Constructor for IncomingInterface.
        :param parent: AyxPlugin
        """

        # Default properties
        self.parent = parent

        # Custom properties
        self.record_copier = None
        self.record_creator = None

    def ii_init(self, record_info_in: object) -> bool:
        """
        Handles appending the new field to the incoming data.
        Called to report changes of the incoming connection's record metadata to the Alteryx engine.
        :param record_info_in: A RecordInfo object for the incoming connection's fields.
        :return: False if there's an error with the field name, otherwise True.
        """

        if not self.parent.is_initialized:
            return False

        # Returns a new, empty RecordCreator object that is identical to record_info_in.
        record_info_out = record_info_in.clone()
        if self.parent.output_field_size:
            self.parent.output_field = record_info_out.add_field(self.parent.output_field_name, self.parent.output_field_type, self.parent.output_field_size)
        else:
            self.parent.output_field = record_info_out.add_field(self.parent.output_field_name, self.parent.output_field_type)
        self.parent.output_anchor.init(record_info_out)

        # Creating a new, empty record creator based on record_info_out's record layout.
        self.record_creator = record_info_out.construct_record_creator()

        # Instantiate a new instance of the RecordCopier class.
        self.record_copier = Sdk.RecordCopier(record_info_out, record_info_in)
        for index in range(record_info_in.num_fields):
            self.record_copier.add(index, index)
        self.record_copier.done_adding()

        # Construct Group Reader
        self.parent.grouping_fields_objects = []
        for grouping_field in self.parent.grouping_fields:
            if grouping_field != '""':
                if record_info_in.get_field_by_name(grouping_field.strip('"')):
                    self.parent.grouping_fields_objects.append(record_info_in.get_field_by_name(grouping_field.strip('"')))
                else:
                    self.alteryx_engine.output_message(self.n_tool_id, Sdk.EngineMessageType.info, 'Failed to find ' + grouping_field.strip('"'))

        return True

    def ii_push_record(self, in_record: object) -> bool:
        """
        Responsible for pushing records out.
        Called when an input record is being sent to the plugin.
        :param in_record: The data for the incoming record.
        :return: False if there's a downstream error, or if there's an error with the field name, otherwise True.
        """

        if not self.parent.is_initialized:
            return False

        # Copy the data from the incoming record into the outgoing record.
        self.record_creator.reset()
        self.record_copier.copy(self.record_creator, in_record)

        # Check Group
        if len(self.parent.grouping_fields_objects) > 0:
            new_group = []
            for grouping_field in self.parent.grouping_fields_objects:
                if grouping_field.type in [Sdk.FieldType.byte, Sdk.FieldType.int16, Sdk.FieldType.int32, Sdk.FieldType.int64]:
                    new_group.append(grouping_field.get_as_int64(in_record))
                elif grouping_field.type in [Sdk.FieldType.float, Sdk.FieldType.double]:
                    new_group.append(grouping_field.get_as_double(in_record))
                else:
                    new_group.append(grouping_field.get_as_string(in_record))
            if self.parent.current_group and self.parent.current_group == new_group:
                self.parent.current_value += 1
            else:
                self.parent.current_value = self.parent.starting_value
                self.parent.current_group = new_group
        else:
            self.parent.current_value += 1

        # Sets the value of this field in the specified record_creator from an int64 value.
        if self.parent.output_field_type == Sdk.FieldType.string or self.parent.output_field_type == Sdk.FieldType.wstring:
            self.parent.output_field.set_from_string(self.record_creator, str(self.parent.current_value).zfill(self.parent.output_field_size))
        else:
            self.parent.output_field.set_from_int64(self.record_creator, self.parent.current_value)

        out_record = self.record_creator.finalize_record()

        # Push the record downstream and quit if there's a downstream error.
        if not self.parent.output_anchor.push_record(out_record):
            return False

        return True

    def ii_update_progress(self, d_percent: float):
        """
        Called by the upstream tool to report what percentage of records have been pushed.
        :param d_percent: Value between 0.0 and 1.0.
        """
        self.parent.alteryx_engine.output_tool_progress(self.parent.n_tool_id, d_percent)
        self.parent.output_anchor.update_progress(d_percent)

    def ii_close(self):
        """
        Called when the incoming connection has finished passing all of its records.
        """
        self.parent.output_anchor.close()
