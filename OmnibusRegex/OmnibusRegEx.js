let plugInHelper
let previewData
const previewElement = document.getElementById('previewResult')
const codeMirrorEditor = CodeMirror.fromTextArea(document.getElementById('regularExpression'), {
  mode: 'regex'
})

document.addEventListener('keydown', (e) => {
  if (e.key.match(/F1/)) {
    e.preventDefault() // prevent the normal help methods from being triggered
    e.stopPropagation()
    plugInHelper.openHelpPage('RegEx.htm')
  }
})

/**
 * Specify actions that will take place before the tool's configuration is loaded into the manager.
 *
 * @param manager The data manager.
 * @param AlteryxDataItems The data items in use on this page.
 * @param json Configuration
 */
Alteryx.Gui.BeforeLoad = function (manager, AlteryxDataItems, json) {
  plugInHelper = PlugInHelper.Create(Alteryx, manager, AlteryxDataItems, window)
  plugInHelper.createDataItem('showPreview', true, true)
  plugInHelper.createDataItem('RegExExpressionTemp', json.Configuration && json.Configuration.RegExExpression ? json.Configuration.RegExExpression['@value'] : '')

  plugInHelper.createDataItem('CaseInsensitiveTemp', !json.Configuration || !json.Configuration.CaseInsensitive || json.Configuration.CaseInsensitive['@value'] !== 'False')

  const methodSelectorItem = new AlteryxDataItems.StringSelector({
    dataname: 'MethodTemp',
    id: 'MethodTemp'
  })

  methodSelectorItem.StringList
    .Add('Match', 'Match')
    .Add('Replace', 'Replace')
    .Add('ParseSimple', 'Split To Rows')
    .Add('ParseSimpleColumns', 'Split To Columns')
  // .Add('ParseComplex', 'Parse')
  methodSelectorItem.setValue(json.Configuration ? json.Configuration.Method : 'Match')
  if (methodSelectorItem.value === 'ParseSimple' && json.Configuration && json.Configuration.ParseSimple && json.Configuration.ParseSimple.SplitToRows && json.Configuration.ParseSimple.SplitToRows['@value'] === 'False') {
    methodSelectorItem.setValue('ParseSimpleColumns')
  }
  manager.AddDataItem(methodSelectorItem)

  plugInHelper.createDataItem('MatchField', json.Configuration && json.Configuration.Match ? json.Configuration.Match.Field : '')
  plugInHelper.createDataItem('ErrorUnmatched', (json.Configuration && json.Configuration && json.Configuration.Match && json.Configuration.Match.ErrorUnmatched && json.Configuration.Match.ErrorUnmatched['@value'] === 'True') || false)
  plugInHelper.createDataItem('ReplaceExpression', json.Configuration && json.Configuration.Replace ? json.Configuration.Replace['@expression'] : '')
  plugInHelper.createDataItem('CopyUnmatched', json.Configuration && json.Configuration.Replace && json.Configuration.Replace.CopyUnmatched ? json.Configuration.Replace.CopyUnmatched['@value'] !== 'False' : true)
  plugInHelper.createDataItem('NumFields', json.Configuration && json.Configuration.ParseSimple && json.Configuration.ParseSimple.NumFields ? +json.Configuration.ParseSimple.NumFields['@value'] : 1)
  plugInHelper.createDataItem('RootName', (json.Configuration && json.Configuration.ParseSimple && json.Configuration.ParseSimple.RootName) || '')

  const parseColumnErrorItem = new AlteryxDataItems.StringSelector({
    dataname: 'ParseColumnError',
    id: 'ParseColumnError'
  })
  parseColumnErrorItem.StringList
    .Add('Warn', 'Drop Extra with Warning')
    .Add('Ignore', 'Drop Extra without Warning')
    .Add('Error', 'Error')
  parseColumnErrorItem.setValue(json.Configuration && json.Configuration.ParseSimple && json.Configuration.ParseSimple.ErrorHandling ? json.Configuration.ParseSimple.ErrorHandling : 'Warn')
  manager.AddDataItem(parseColumnErrorItem)
}

/**
 * Specify actions that will take place before the tool's configuration is loaded into the manager.
 *
 * @param manager The data manager.
 * @param AlteryxDataItems The data items in use on this page.
 */
Alteryx.Gui.AfterLoad = function (manager, AlteryxDataItems) {
  function reevaluate (fieldName, method, regex, caseInsensitive, replace) {
    let fail = false
    const flags = caseInsensitive ? 'gi' : 'g'

    try {
      if (!previewData || !previewData[fieldName]) {
        fail = true
        previewElement.textContent = 'No Preview Data Available. Run the workflow.'
      } else if (method === 'Match') {
        previewElement.textContent = new RegExp(regex, flags).test(previewData[fieldName]) + ' '
      } else if (method === `Replace`) {
        previewElement.textContent = previewData[fieldName].replace(new RegExp(regex, flags), replace) + ' '
      } else if (method === 'ParseSimple') {
        const matches = previewData[fieldName].match(new RegExp(regex, flags))
        previewElement.textContent = matches.length ? matches.join('\r\n') : 'null'
      } else if (method === 'ParseSimpleCol') {
        const matches = previewData[fieldName].match(new RegExp(regex, flags))
        previewElement.textContent = matches.length ? matches.join('\r\n') : 'null'
      } else {
        fail = true
        previewElement.textContent = `${method} preview not supported yet`
      }
    } catch (err) {
      previewElement.textContent = err.message
    }

    previewElement.style.color = fail ? '#cc0000' : '#333'
  }

  const showPreviewItem = manager.GetDataItem('showPreview')
  const showPreviewChanged = (v) => {
    document.getElementById('preview').style.display = (v ? 'block' : 'none')
    document.getElementById('previewRegex').style.display = (v ? 'block' : 'none')
  }
  showPreviewItem.BindUserDataChanged(showPreviewChanged)

  const fieldItem = manager.GetDataItem('Field')
  const methodItem = manager.GetDataItem('MethodTemp')
  const regExExpressionItem = manager.GetDataItem('RegExExpressionTemp')
  const caseInsensitiveItem = manager.GetDataItem('CaseInsensitiveTemp')
  const replaceItem = manager.GetDataItem('ReplaceExpression')
  const numFields = manager.GetDataItem('NumFields')
  const rootName = manager.GetDataItem('RootName')

  const callReevaluate = () => {
    reevaluate(
      fieldItem.value,
      methodItem.value,
      regExExpressionItem.value,
      caseInsensitiveItem.value,
      replaceItem.value,
      numFields.value,
      rootName.value)
  }
  const getFieldPreview = v => {
    document.getElementById('preview').textContent = (v && previewData ? previewData[v] : ' ')
  }

  plugInHelper.getInputDataArray('', 1, d => {
    previewData = d[0]
    getFieldPreview(fieldItem.value)
    callReevaluate()
  })

  const methodChanged = (v) => {
    document.getElementById('MatchFieldSet').style.display = (v === 'Match' ? 'block' : 'none')
    document.getElementById('ReplaceFieldSet').style.display = (v === 'Replace' ? 'block' : 'none')
    document.getElementById('ColumnFieldSet').style.display = (v === 'ParseSimpleColumns' ? 'block' : 'none')
    callReevaluate()
  }

  codeMirrorEditor.getDoc().setValue(regExExpressionItem.value)
  codeMirrorEditor.on('change', () => regExExpressionItem.setValue(codeMirrorEditor.getDoc().getValue()))

  methodItem.BindUserDataChanged(methodChanged)
  fieldItem.BindUserDataChanged(getFieldPreview)
  caseInsensitiveItem.BindUserDataChanged(callReevaluate)
  regExExpressionItem.BindDataChanged(callReevaluate)
  replaceItem.BindUserDataChanged(callReevaluate)
  rootName.BindUserDataChanged(callReevaluate)

  methodChanged(methodItem.value)
}

/**
 * Set the tool's default annotation on the canvas.
 *
 * @param manager The data manager.
 * @returns {string}
 */
Alteryx.Gui.Annotation = function (manager, AlteryxDataItems) {
  const methodItem = manager.GetDataItem('Method')
  const methodName = methodItem.StringList.enums.filter(e => e.dataName === methodItem.value)[0].uiObject

  const regexText = manager.GetDataItem('RegExExpressionTemp').value
  const suffixText = methodName === `Replace` ? `\n${manager.GetDataItem('ReplaceExpression').value}` : ''
  return `${methodName}:\n${plugInHelper.truncateString(regexText)}${plugInHelper.truncateString(suffixText)}`
}

/**
 * Reformat the JSON to the style we need
 */
Alteryx.Gui.BeforeGetConfiguration = function (json) {
  json.Configuration.CaseInsensitive = [{
    '@value': json.Configuration.CaseInsensitiveTemp
  }]
  delete json.Configuration.CaseInsensitiveTemp

  json.Configuration.RegExExpression = [{
    '@value': json.Configuration.RegExExpressionTemp
  }]
  delete json.Configuration.RegExExpressionTemp

  // Match
  json.Configuration.Match = {
    Field: json.Configuration.MatchField || (json.Configuration.Field ? `${json.Configuration.Field}_Matched` : ''),
    ErrorUnmatched: [{
      '@value': json.Configuration.ErrorUnmatched
    }]
  }
  delete json.Configuration.MatchField
  delete json.Configuration.ErrorUnmatched

  // Replace
  json.Configuration.Replace = [{
    '@expression': json.Configuration.ReplaceExpression,
    'CopyUnmatched': [{
      '@value': json.Configuration.CopyUnmatched
    }]
  }]
  delete json.Configuration.ReplaceExpression
  delete json.Configuration.CopyUnmatched

  // Parse Simple
  json.Configuration.ParseSimple = {
    SplitToRows: [{
      '@value': json.Configuration.Method === 'ParseSimple' ? 'True' : 'False'
    }],
    NumFields: [{
      '@value': json.Configuration.NumFields
    }],
    ErrorHandling: json.Configuration.ParseColumnError,
    RootName: json.Configuration.RootName || json.Configuration.Field
  }
  delete json.Configuration.NumFields
  delete json.Configuration.ParseColumnError
  delete json.Configuration.RootName

  json.Configuration.Method = json.Configuration.MethodTemp
  if (json.Configuration.Method.match(/ParseSimple.*/)) {
    json.Configuration.Method = 'ParseSimple'
  }
  delete json.Configuration.MethodTemp

  console.log(json.Configuration)
  return json
}