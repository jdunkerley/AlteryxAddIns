/// <reference path="../AlteryxDesigner.d.ts" />

Alteryx.Gui.BeforeLoad = (manager, dataItems) => {
  if (!manager.GetDataItemByDataName('ColumnName')) {
    manager.AddDataItem(new dataItems.SimpleString({ dataname: 'ColumnName', id: 'ColumnName', initialValue: 'HelloFromJS' }))
  }
  if (!manager.GetDataItemByDataName('Value')) {
    manager.AddDataItem(new dataItems.SimpleString({ dataname: 'Value', id: 'Value', initialValue: 'Hello From JavaScript' }))
  }
}

Alteryx.Gui.AfterLoad = (manager) => {
  const ColumnNameItem: AlteryxDataItems.SimpleString = manager.GetDataItemByDataName('ColumnName')
  ColumnNameItem.BindUserDataChanged((newValue) => {
    const newString: string = (newValue || '').toString()
    const element = document.getElementById('fieldNameValid')
    if (element) element.innerHTML = (newString.trim() === '') ? 'You must enter a field name' : ''
  })
}

Alteryx.Gui.Annotation = (manager) => {
  const ColumnNameItem: AlteryxDataItems.SimpleString = manager.GetDataItemByDataName('ColumnName')
  const ValueItem: AlteryxDataItems.SimpleString = manager.GetDataItemByDataName('Value')
  return `${(ColumnNameItem.getValue())}:\r\n${ValueItem.getValue().replace(/^(.{27}).{3,}/, '$1...')}`
}

Alteryx.Gui.BeforeGetConfiguration = (json) => {
  if ((json.Configuration.ColumnName || '').trim() === '') {
    json.Configuration.ColumnName = 'NewJSColumn'
    json.Annotation = `NewJSColumn:\r\n${(json.Annotation || '').replace(/^.*?:\r\n/, '')}`
  }
  return json
}
