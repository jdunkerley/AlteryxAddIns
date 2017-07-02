/// <reference path="../AlteryxEngine.d.ts" />
const globalConfiguration = {
  state: '',
  columnName: 'HelloFromJS',
  value: 'Hello from JavaScript'
}

Alteryx.Plugin.DefineConnections = () => {
  console.log('DefineConnections Called.')
  return {
    IncomingConnections: [{ type: 'Input' }],
    OutgoingConnections: [{ name: 'Output' }]
  }
}

Alteryx.Plugin.PI_Init = (config) => {
  console.log('PI_Init Called.')
  globalConfiguration.state = 'Inited'
  if (config.Configuration) {
    globalConfiguration.columnName = config.Configuration['ColumnName'] || globalConfiguration.columnName
    globalConfiguration.value = config.Configuration['Value'] || globalConfiguration.value
  }
}

Alteryx.Plugin.II_Init = (metaInfo) => {
  console.log(`II_Init Called: ${metaInfo.Connection}.`)
  globalConfiguration.state = 'Connected'
  const newField: Alteryx.RecordFieldInfo = { name: globalConfiguration.columnName, type: 'V_WString', size: 64532 }
  Alteryx.Engine.SendMessage.RecordInfo('Output', { Field: [...metaInfo.RecordInfo.Field, newField] })
}

Alteryx.Plugin.II_PushRecords = (data) => {
  console.log(`II_PushRecords Called: ${data.Connection}.`)
  Alteryx.Engine.SendMessage.PushRecords('Output', data.Records.map(r => [...r, globalConfiguration.value]))
  Alteryx.JsEvent(JSON.stringify({ Event: 'PushRecords', Connection: 'Output', ToolProgress: data.Progress, Records: '[]' }))
}

Alteryx.Plugin.II_AllClosed = () => {
  console.log('II_AllClosed Called.')
  globalConfiguration.state = 'Closed.'
  Alteryx.Engine.SendMessage.CloseOutput('Output')
  Alteryx.Engine.SendMessage.Complete()
}

Alteryx.Plugin.PI_Close = () => {
  console.log('PI_Close Called.')
  if (globalConfiguration.state === 'Inited') {
    Alteryx.Engine.SendMessage.Error('HelloFromJS requires an Input connection')
  }
  Alteryx.Engine.SendMessage.PI_Close()
}
