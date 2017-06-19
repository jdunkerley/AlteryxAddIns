// ToDo: fieldinfo.jsx
// ToDo: manager.jsx
// ToDo: FieldSelectorMulti
// ToDo: MultiStringSelector
// ToDo: FormulaData
// ? ToDo: DataItemContainer
// ? ToDo: GridData
// ? ToDo: LayoutData
// ? ToDo: SalesforceGridData
// ? ToDo: SimpleGraph
// ? ToDo: SimpleGraphData
// ? ToDo: SimpleLineGraph 

declare namespace Alteryx {

    interface AlteryxGui {
        BeforeLoad(manager: AlteryxDataManager, dataItems: any, jsonConfiguration: ToolConfiguration) => void
    }

    export class FieldInfo {
        constructor (_name: string, _type: FieldType, _size: number, _scale: number, _source: string, _desc: string)
        strName: string
        strType: FieldType
        nSize: number
        nScale: number
        strSource: string
        strDescription: string
        GetFullSize(): string
    }

    export class FieldInfoUnknown extends FieldInfo {
        constructor (_name: string)
    }

    export class FieldInfoManual extends FieldInfo {
        constructor (_name: string, _type: FieldType, _size: number, _scale: number, _source: string, _desc: string)
    }

    declare type FieldType = "Blob" | "Bool" | "Byte" | "Int16" | "Int32" | "Int64" | "FixedDecimal" | "Float" | "Double" | "String" | "WString" | "V_String" | "V_WString" | "Date" | "Time" | "DateTime" | "SpatialObj" | "Unknown"

    export class DW2FieldType {
        static IsBool(FieldType): boolean
        static IsNumeric(FieldType): boolean
        static IsString(FieldType): boolean
        static IsFloat(FieldType): boolean
        static IsInteger(FieldType): boolean
        static IsStringOrDate(FieldType): boolean
        static IsDateOrTime(FieldType): boolean
        static IsDate(FieldType): boolean
        static IsBinary(FieldType): boolean
        static IsSpatialObj(FieldType): boolean
        static GetDefaultSize(FieldType): number
    }

    declare const Gui : AlteryxGui

    export class FieldList {

    }

    export class Manager {
        CreateField(name: string, type: FieldType, size?: number, scale?: number, source?: string, desc?: string): FieldInfoManual
        GetDataItems(): AlteryxDataItems.DataItem[]
    }
}

declare namespace AlteryxDataItems {
    interface DataItemArgs {
        dataname: string
        id: string
    }

    interface SimpleBoolArgs extends DataItemArgs {
        ischecked?: boolean
    }

    interface SimpleNumberArgs extends DataItemArgs {
        initialvalue?: number
    }

    interface SimpleStringArgs extends DataItemArgs {
        initialvalue?: string
        password?: 'true' | 'false'
        isEncrypted?: boolean
    }

    interface FileBrowseArgs extends DataItemArgs {
        browseType: 'File' | 'Folder'
        isWorkflowDependency: boolean
    }

    interface FieldSelectorArgs extends DataItemArgs {
        inputNumber: number
        connectionNumber: number
        includenone: "True" | "False"
        fieldtype: "All" | "NoBinary" | "NoBlob" | "NoSpatial" | "String" | "Date" | "DateOrTime" | "StringOrDate" | "Numeric" | "SpatialObj" | "Bool" | "Time" | "Blob"
        onChangeHandler: (dataItem: DataItem,isNewField: boolean) => void
        customFields?: string[]
    }

    export class DataItem {
        constructor(dataname: string, id: string)

        dataName: string
        id: string
        suppressed: boolean

        SetDependency(id: string, dataItem: DataItem): void
        GetDependency(id: string): DataItem

        BindDataChanged(func: (newValue:any) => void): void
        BindUserDataChanged(func: (newValue:any) => void): void

        StringToBoolean(input: string): boolean
        BooleanToString(input: boolean): string
    }

    export class SimpleBool extends DataItem {
        constructor(args: SimpleBoolArgs)
        getValue(): boolean
        setValue(newValue: boolean, updateWidgetUI?: boolean): void
    }

    export class SimpleString extends DataItem {
        constructor(args: SimpleStringArgs)
        getValue(): string
        setValue(newValue: string | null | undefined, updateWidgetUI?: boolean): void
    }

    export class SimpleInt extends DataItem {
        constructor(args: SimpleNumberArgs)
        getValue(): number
        setValue(newValue: string | number, updateWidgetUI?: boolean): void
    }

    export class SimpleFloat extends DataItem {
        constructor(args: SimpleNumberArgs)
        getValue(): number
        setValue(newValue: string | number, updateWidgetUI?: boolean): void
    }

    export class SimpleDate extends DataItem {
        constructor(args: DataItemArgs)
        dateFormat?: string
        getValue(): string
        setValue(newValue: number | string, updateWidgetUI?: boolean): void
    }

    export class SimpleTime extends DataItem {
        constructor(args: DataItemArgs)
        timeFormat?: string
        getValue(): string
        setValue(newValue: number | string, updateWidgetUI?: boolean): void
    }

    export class SimpleDateTime extends DataItem {
        constructor(args: DataItemArgs)
        dateFormat?: string
        timeFormat?: string
        dateTimeFormat?: string
        getValue(): string
        setValue(newValue: number | string, updateWidgetUI?: boolean): void
    }

    interface EnumStringValue {
        dataname: string
        uiobject?: string
        default?: boolean   
    }

    export class StringSelector extends DataItem {
        constructor(args: DataItemArgs)
        setStringList(newStringList: EnumStringValue[], updateWidgetUI?: boolean): void
        FieldExists(fieldName: string): boolean
        setValue(newValue: string | null | undefined, updateWidgetUI?: boolean): void
    }

    export class FileBrowseData extends DataItem {
        constructor(args: FileBrowseArgs)
        setValue(newValue: number | string, updateWidgetUI?: boolean): void
    }

    export class FieldSelector extends DataItem {
        constructor(args: FieldSelectorArgs, manager: Alteryx.Manager)
        GetFieldList(): Alteryx.FieldList
        FieldExists(fieldName: string): boolean
        GetFieldStatus(fieldName: string): '' | ' (Missing)' | ' (Bad Field Type)'
        ChangeFieldList(fieldList: Alteryx.FieldList, bRemoveForcedFields?: boolean): void
        ChangeFieldFilter(fieldFilter: (fi: Alteryx.FieldInfo) => boolean, bRemoveForcedFields?: boolean): void
        ForceFieldInList(field: Alteryx.FieldInfo): void
        IsForcedField(fieldName: string): boolean
        setValue(newValue: string | null | undefined, updateWidgetUI?: boolean, isAddFieldOption?: boolean): void
    }
}


interface ToolConfiguration {
    Configuration: any
}


interface AlteryxDataManager {
    toolId: number
    toolName: string
    AddDataItem<Item extends DataItem>(item:Item) => void
}
