/**
 * Specify actions that will take place before the tool's configuration is loaded into the manager.
 * @param manager The data manager.
 * @param AlteryxDataItems The data items in use on this page.
 * @param json Configuration
 */
Alteryx.Gui.BeforeLoad = (manager, AlteryxDataItems, json) => {
  const dataFieldNameItem = new AlteryxDataItems.SimpleString('FieldName', {})
  dataFieldNameItem.setValue('RecordID')
  manager.addDataItem(dataFieldNameItem)
  manager.bindDataItemToWidget(dataFieldNameItem, 'dataFieldName')

  const dataLastColumnItem = new AlteryxDataItems.SimpleBool('LastColumn', {})
  dataLastColumnItem.setValue(true)
  manager.addDataItem(dataLastColumnItem)
  manager.bindDataItemToWidget(dataLastColumnItem, 'dataLastColumn')

  const dataFieldTypeItem = new AlteryxDataItems.StringSelector('FieldType', {
    optionList: [
      'Byte',
      'Int16',
      'Int32',
      'Int64',
      'String',
      'WString',
      'V_String',
      'V_WString'
    ].map(a => { return {label: a, value: a} })
  })
  dataFieldTypeItem.setValue('Int64')
  manager.addDataItem(dataFieldTypeItem)
  manager.bindDataItemToWidget(dataFieldTypeItem, 'dataFieldType')

  const dataStringSize = new AlteryxDataItems.ConstrainedInt('StringSize', {min: 1, max: 1073741823})
  dataStringSize.setValue(5)
  manager.addDataItem(dataStringSize)
  manager.bindDataItemToWidget(dataStringSize, 'dataStringSize')

  const dataStartingValue = new AlteryxDataItems.ConstrainedInt('StartingValue', {})
  dataStartingValue.setValue(1)
  manager.addDataItem(dataStartingValue)
  manager.bindDataItemToWidget(dataStartingValue, 'dataStartingValue')

  const dataGroupingFields = new AlteryxDataItems.FieldSelectorMulti('GroupingFields', {manager: manager, connectionIndex: 0, anchorIndex: 0, delimiter: '","'})
  setJsonSerialiser(dataGroupingFields)
  manager.addDataItem(dataGroupingFields)
  manager.bindDataItemToWidget(dataGroupingFields, 'dataGroupingFields')

  const dataSortingFields = new AlteryxDataItems.FieldSelectorMulti('SortingFields', {manager: manager, connectionIndex: 0, anchorIndex: 0, delimiter: '","'})
  setJsonSerialiser(dataSortingFields)
  manager.addDataItem(dataSortingFields)
  manager.bindDataItemToWidget(dataSortingFields, 'dataSortingFields')

  const dataDescendingFields = new AlteryxDataItems.FieldSelectorMulti('DescendingFields', {manager: manager, connectionIndex: 0, anchorIndex: 0, delimiter: '","'})
  setJsonSerialiser(dataDescendingFields)
  manager.addDataItem(dataDescendingFields)
  manager.bindDataItemToWidget(dataDescendingFields, 'dataDescendingFields')
}

function setJsonSerialiser (item) {
  const innerFn = item.fromJson
  item.fromJson = (e, t, n) => (typeof n === 'string' && innerFn(e, t, n.replace(/(^")|("$)/g, '')))
  item.toJson = (e, t) => e({ DataItem: `"${item.getValue().join(item.getDelimiter())}"`, DataName: item.getDataName() })
}

/**
 * Specify actions that will take place before the tool's configuration is loaded into the manager.
 * @param manager The data manager.
 * @param AlteryxDataItems The data items in use on this page.
 */
Alteryx.Gui.AfterLoad = (manager, AlteryxDataItems) => {
}

/**
 * Reformat the JSON to the style we need
 * @param json Configuration
 */
Alteryx.Gui.BeforeGetConfiguration = (json) => {
  return json
}

/**
 * Set the tool's default annotation on the canvas.
 * @param manager The data manager.
 * @returns {string}
 */
Alteryx.Gui.Annotation = (manager) => ''