// ToDo: Renderer (and associated Manager functions)
// ToDo: FormulaData
// ToDo: DataItemContainer
// ToDo: LayoutData
// ToDo: GridData
// ToDo: SalesforceGridData
// ToDo: SimpleGraph
// ToDo: SimpleGraphData
// ToDo: SimpleLineGraph 
declare module Alteryx {
  export type FieldType = "Blob" | "Bool" | "Byte" | "Int16" | "Int32" | "Int64" | "FixedDecimal" | "Float" | "Double" | "String" | "WString" | "V_String" | "V_WString" | "Date" | "Time" | "DateTime" | "SpatialObj" | "Unknown"
  export class FieldInfo {
    constructor(_name: string, _type: FieldType, _size: number, _scale: number, _source: string, _desc: string)
    strName: string
    strType: FieldType
    nSize: number
    nScale: number
    strSource: string
    strDescription: string
    GetFullSize(): string
  }

  export class FieldInfoUnknown extends FieldInfo {
    constructor(_name: string)
  }

  export class FieldInfoManual extends FieldInfo {
    constructor(_name: string, _type: FieldType, _size: number, _scale: number, _source: string, _desc: string)
  }

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

  export class FieldList {
    constructor(eRecordInfo?: any, connectionName?: string)
    _GetFields(): FieldInfo[]
    AddField(fi: FieldInfo, isAddFieldOption?: boolean): void
    CombineFields(fieldList: FieldList): void
    SelectFieldTypes_All(fi: FieldInfo): boolean
    SelectFieldTypes_NoBinary(fi: FieldInfo): boolean
    SelectFieldTypes_NoBlob(fi: FieldInfo): boolean
    SelectFieldTypes_NoSpatial(fi: FieldInfo): boolean
    SelectFieldTypes_String(fi: FieldInfo): boolean
    SelectFieldTypes_Date(fi: FieldInfo): boolean
    SelectFieldTypes_DateOrTime(fi: FieldInfo): boolean
    SelectFieldTypes_StringOrDate(fi: FieldInfo): boolean
    SelectFieldTypes_Numeric(fi: FieldInfo): boolean
    SelectFieldTypes_SpatialObj(fi: FieldInfo): boolean
    SelectFieldTypes_Bool(fi: FieldInfo): boolean
    SelectFieldTypes_Time(fi: FieldInfo): boolean
    SelectFieldTypes_Blob(fi: FieldInfo): boolean
    GetField(strName: string, bForce: boolean, bThrowException?: boolean, isAddFieldOption?: boolean): FieldInfo
    GetFieldList_All(): FieldInfo[]
    GetFieldList_NoBinary(): FieldInfo[]
    GetFieldList_NoBlob(): FieldInfo[]
    GetFieldList_NoSpatial(): FieldInfo[]
    GetFieldList_String(): FieldInfo[]
    GetFieldList_Numeric(): FieldInfo[]
    GetFieldList_SpatialObj(): FieldInfo[]
  }

  export class FieldListArray {
    constructor(eRecordInfo?: any)
    Count: number
    Get(nInput: number, nIndex?: number): FieldList | undefined
    GetCopy(nInput: number, nIndex?: number): FieldList | undefined
    GetCountMultiInputs(nInput: number): number
    GetMultiInputs(nInput: number): FieldList
  }

  export class Manager {
    constructor(incomingMetaInfo: any, renderer: any)
    GetMetaInfo(input: number[]): FieldListArray | FieldList | undefined
    GetMetaInfoCopy(input: number[]): FieldListArray | FieldList | undefined
    CreateField(name: string, type: FieldType, size?: number, scale?: number, source?: string, desc?: string): FieldInfoManual
    GetDataItems(): AlteryxDataItems.DataItem[]
    GetDataItem(id: string): AlteryxDataItems.DataItem
    GetDataItemByDataName(name: string): AlteryxDataItems.DataItem
    AddDataItem(item: AlteryxDataItems.DataItem): void
    RemoveDataItem(item: AlteryxDataItems.DataItem): void
    RemoveDataItemByDataName(dataName: string): void

    toolId: number
    toolName: string

    macroMode: boolean
    isFirstConfig: boolean
  }

  export const AlteryxVersion: string
  export const SDKVersion: string
  export const Platform: string
  export const LibDir: string

  interface recentAndSavedExpressions {
    recentExpressions: string[]
    savedExpressions: string[]
  }

  interface beforeGetConfiguration {
    Annotation?: string
    Configuration: any 
  }

  interface AlteryxGui {
    manager: Manager
    renderer: any

    // Not sure any of these need exposing
    Initialize(config: any): any
    GetConfiguration(): void
    SetConfiguration(args: any): any
    AttachObserver(): void

    // Needs to be set up (see Formula.tsx data type)
    getRecentAndSavedExpressions: () => void
    setRecentAndSavedExpressions: (expressions: recentAndSavedExpressions) => void

    // Plug In Methods
    BeforeLoad?: (manager: Manager, dataItems: AlteryxDataItems, json: any) => void
    AfterLoad?: (manager: Manager, dataItems: AlteryxDataItems) => void
    BeforeGetConfiguration?: (json: beforeGetConfiguration) => any
    Annotation?: (manager: Manager) => string
  }

  export function JsEvent(jsonObject: string): void

  export const Gui: AlteryxGui
}

declare module AlteryxDataItems {
  interface DataItemArgs {
    dataname: string
    id: string
  }

  interface SimpleBoolArgs extends DataItemArgs {
    ischecked?: boolean
  }

  interface SimpleNumberArgs extends DataItemArgs {
    initialValue?: number
  }

  interface MultiStringArgs extends DataItemArgs {
    delimeter: string
  }

  interface SimpleStringArgs extends DataItemArgs {
    initialValue?: string
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
    onChangeHandler: (dataItem: DataItem, isNewField: boolean) => void
    customFields?: string[]
  }

  interface FieldSelectorMultiArgs extends FieldSelectorArgs {
    delimeter: string
  }

  export class DataItem {
    constructor(dataname: string, id: string)

    dataName: string
    id: string
    suppressed: boolean

    getValue(): any
    setValue(newValue: any, updateWidgetUI?: boolean): void

    SetDependency(id: string, dataItem: DataItem): void
    GetDependency(id: string): DataItem

    BindDataChanged(func: (newValue: any) => void): void
    BindUserDataChanged(func: (newValue: any) => void): void

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
    StringListChanged: (newStringList: EnumStringValue[]) => void[]
    setStringList(newStringList: EnumStringValue[], updateWidgetUI?: boolean): void
    FieldExists(fieldName: string): boolean
    setValue(newValue: string | null | undefined, updateWidgetUI?: boolean): void
  }

  export class MultiStringSelector extends StringSelector {
    constructor(args: MultiStringArgs)
    setValue(newValue: string[] | string | null | undefined, updateWidgetUI?: boolean): void
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

  export class FieldSelectorMulti extends DataItem {
    constructor(args: FieldSelectorMultiArgs, manager: Alteryx.Manager)
    GetFieldList(): Alteryx.FieldList
    GetFieldStatus(fieldName: string): '' | ' (Missing)' | ' (Bad Field Type)'
    ChangeFieldList(fieldList: Alteryx.FieldList, bRemoveForcedFields?: boolean): void
    ChangeFieldFilter(fieldFilter: (fi: Alteryx.FieldInfo) => boolean, bRemoveForcedFields?: boolean): void
    ForceFieldInList(field: Alteryx.FieldInfo): void
    setValue(newValue: string[] | string | null | undefined, updateWidgetUI?: boolean, isAddFieldOption?: boolean): void
  }
}
