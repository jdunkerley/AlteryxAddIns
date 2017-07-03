declare module Alteryx {
  export type FieldType = "Blob" | "Bool" | "Byte" | "Int16" | "Int32" | "Int64" | "FixedDecimal" | "Float" | "Double" | "String" | "WString" | "V_String" | "V_WString" | "Date" | "Time" | "DateTime" | "SpatialObj" | "Unknown"

  interface SendMessage {
    Info(message: string): void
    Warning(message: string): void
    Error(message: string): void
    FieldConversionError(message: string): void

    GetCachedMetaInfo(connection: string, callback: (metaInfo: any) => void)

    RecordInfo(connection: string, recordInfo: RecordInfo): void
    PushRecords(outputName: string, records: any[][]): void
    CloseInput(inputName: string): void
    CloseOutput(outputName: string): void
    PI_Close(): void
    Complete(): void
  }

  interface AlteryxEngine {
    SendMessage: SendMessage
    Callbacks: () => void[] | undefined
  }

  interface RecordFieldInfo {
    name: string
    type: FieldType
    source?: string
    description?: string
    size?: number
    scale?: number
  }

  interface RecordInfo {
    Field: RecordFieldInfo[]
  }

  interface MetaInfo {
    Connection: string
    RecordInfo: RecordInfo
  }

  interface RecordData {
    Connection: string
    Progress: number
    Records: any[][]
  }

  export const AlteryxVersion: string
  export const SDKVersion: string
  export const Platform: string
  export const LibDir: string

  export function JsEvent(jsonObject: string): void

  export const Engine: AlteryxEngine

  interface SortInfo {
    field: string
    order: "Asc" | "Desc"
  }

  interface IncomingConnection {
    type: string
    SortInfo?: SortInfo[]
  }

  interface OutgoingConnection {
    name: string
  }

  interface Connections {
    IncomingConnections: IncomingConnection[]
    OutgoingConnections: OutgoingConnection[]
  }

  interface Configuration {
    Configuration?: any
    SortInfo?: any[]
  }

  interface AlteryxPlugin {
    /**
     * This function defines our input and output connections.
     * It must match the input and output connections defined in the GUI plugin's XML file.
     */
    DefineConnections: () => Connections

    /**
     * Called at the beginning of plugin lifetime with the plugin's configuration properties.
     */
    PI_Init: (config: Configuration) => void

    /**
     * Called once for each incoming connection with the connection's metainfo. When a per-connection init comes in,
     * we would probably store off the incoming RecordInfo.
     */
    II_Init: (metaInfo: Alteryx.MetaInfo) => void

    /**
     * After II_Init has been called for each incoming connection, II_PushRecords is called for each non-empty
     * incoming connection with that connection's records. This implementation contains example code that
     * pushes out the same records it receives.
     */
    II_PushRecords: (data: Alteryx.RecordData) => void

    /**
     * II_AllClosed is called with no arguments after all incoming connections have closed. This implementation
     * sends a CloseOutput message with the name of the outgoing connection to close.
     * All code paths must terminate with a call to Alteryx.Engine.SendMessage.Complete()
     */
    II_AllClosed: () => void

    /**
     * If the tool has no input:
     *    PI_PushAllRecords is called instead of the II functions.
     *    It is also called at configure time with a record limit of 0.
     * All code paths must terminate with a call to Alteryx.Engine.SendMessage.Complete()
     */
    PI_PushAllRecords: (recordLimit: number) => void

    /**
     * PI_Close is called with no arguments at the end of the plugin's lifetime.
     */
    PI_Close: () => void
  }

  export const Plugin: AlteryxPlugin
}