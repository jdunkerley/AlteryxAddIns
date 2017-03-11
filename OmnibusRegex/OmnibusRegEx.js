let plugInHelper
let previewData
const previewElement = document.getElementById('previewResult')
const codeMirrorEditor = CodeMirror.fromTextArea(document.getElementById('regularExpression'), {
    mode: "regex"
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
Alteryx.Gui.BeforeLoad = function(manager, AlteryxDataItems, json) {
    const showPreviewItem = new AlteryxDataItems.SimpleBool({dataname: 'showPreview', id: 'showPreview'})
    showPreviewItem.suppressed = true
    showPreviewItem.setValue(true)
    manager.AddDataItem(showPreviewItem)

    const regExExpressionItem = new AlteryxDataItems.SimpleString({dataname: 'RegExExpressionTemp', id: 'RegExExpressionTemp'})
    regExExpressionItem.setValue(json.Configuration && json.Configuration.RegExExpression ? json.Configuration.RegExExpression['@value'] : '')
    manager.AddDataItem(regExExpressionItem)

    const caseInsensitiveItem = new AlteryxDataItems.SimpleBool({dataname: 'CaseInsensitive', id: 'CaseInsensitive'})
    caseInsensitiveItem.setValue(!json.Configuration || json.Configuration.CaseInsensitive['@value'] === 'True')
    manager.RemoveDataItem(caseInsensitiveItem.dataName)
    manager.AddDataItem(caseInsensitiveItem)

    const methodSelectorItem = new AlteryxDataItems.StringSelector({dataname: 'Method', id: 'Method'})
    methodSelectorItem.StringList
        .Add('Match', 'Match')
        .Add('Replace', 'Replace')
        .Add('ParseSimple', 'Split To Rows')
        // .Add('ParseSimpleColumns', 'Split To Columns')
        // .Add('ParseComplex', 'Parse')
    methodSelectorItem.setValue(json.Configuration ? json.Configuration.Match : 'Match')
    manager.AddDataItem(methodSelectorItem)

    const matchFieldItem = new AlteryxDataItems.SimpleString({dataname: 'MatchField', id: 'MatchField'})
    matchFieldItem.setValue(json.Configuration && json.Configuration.Match ? json.Configuration.Match.Field : '')
    manager.AddDataItem(matchFieldItem)

    const replaceItem = new AlteryxDataItems.SimpleString({dataname: 'ReplaceExpression', id: 'ReplaceExpression'})
    replaceItem.setValue(json.Configuration && json.Configuration.Replace ? json.Configuration.Replace['@expression'] : '')
    manager.AddDataItem(replaceItem)
}

/**
 * Specify actions that will take place before the tool's configuration is loaded into the manager.
 *
 * @param manager The data manager.
 * @param AlteryxDataItems The data items in use on this page.
 */
Alteryx.Gui.AfterLoad = function(manager, AlteryxDataItems){
    plugInHelper = PlugInHelper.Create(Alteryx, manager, window)

    const showPreviewItem = manager.GetDataItem('showPreview')
    const showPreviewChanged = (v) => {
        document.getElementById('preview').style.display = (v ? 'block' : 'none')
        document.getElementById('previewRegex').style.display = (v ? 'block' : 'none')
    }
    showPreviewItem.BindUserDataChanged(showPreviewChanged)

    const fieldItem = manager.GetDataItem('Field')
    const methodItem = manager.GetDataItem('Method')
    const regExExpressionItem = manager.GetDataItem('RegExExpressionTemp')
    const caseInsensitiveItem = manager.GetDataItem('CaseInsensitive')
    const replaceItem = manager.GetDataItem('ReplaceExpression')
    const callReevaluate = () => reevaluate(fieldItem.value, methodItem.value, regExExpressionItem.value, caseInsensitiveItem.value, replaceItem.value)

    const getFieldPreview = (v) => document.getElementById('preview').textContent = (v && previewData ? previewData[v] : ' ')
    plugInHelper.getInputDataArray('', 1, d => {
        previewData = d[0]
        getFieldPreview(fieldItem.value)
        callReevaluate()
    })
    fieldItem.BindUserDataChanged(getFieldPreview)

    const methodChanged = (v) => {
        document.getElementById('MatchFieldSet').style.display = (v === 'Match' ? 'block' : 'none')
        document.getElementById('ReplaceFieldSet').style.display = (v === 'Replace' ? 'block' : 'none')
        callReevaluate()
    }
    methodItem.BindUserDataChanged(methodChanged)

    codeMirrorEditor.getDoc().setValue(regExExpressionItem.value)
    codeMirrorEditor.on('change', () => {
        regExExpressionItem.setValue(codeMirrorEditor.getDoc().getValue())
        callReevaluate()
    })

    regExExpressionItem.BindDataChanged((v) => console.log(v))

    methodChanged(methodItem.value)
}

/**
 * Set the tool's default annotation on the canvas.
 *
 * @param manager The data manager.
 * @returns {string}
 */
Alteryx.Gui.Annotation = function(manager, AlteryxDataItems) { 
    const methodItem = manager.GetDataItem('Method')
    const methodName = methodItem.StringList.enums.filter(e => e.dataName === methodItem.value)[0].uiObject

    const regexText = manager.GetDataItem('RegExExpressionTemp').value
    const suffixText = methodName === `Replace` ? `\n${manager.GetDataItem('ReplaceExpression').value}` : ''
    return `${methodName}:\n${regexText.Length > 30 ? regexText.substring(0, 27)+ '...' : regexText}${suffixText.Length > 30 ? suffixText.substring(0, 27) + '...' : suffixText}`
}

/**
 * Reformat the JSON to the style we need
 */
Alteryx.Gui.BeforeGetConfiguration = function (json) {
    json.Configuration.CaseInsensitive = [{'@value': json.Configuration.CaseInsensitive}]
    json.Configuration.RegExExpression = [{'@value': json.Configuration.RegExExpressionTemp}]
    delete json.Configuration.RegExExpressionTemp

    // Match
    json.Configuration.Match = {
        Field: json.Configuration.MatchField || `${json.Configuration.Field}_Matched`,
        ErrorUnmatched: [{'@value': json.Configuration.ErrorUnmatched}]
    }
    delete json.Configuration.MatchField
    delete json.Configuration.ErrorUnmatched

    // Replace
    json.Configuration.Replace = [{
        '@expression': json.Configuration.ReplaceExpression,
        'CopyUnmatched': [{'@value': true}]
    }]
    delete json.Configuration.ReplaceExpression

    // Parse Simple
    json.Configuration.ParseSimple = {
        SplitToRows: [{'@value': json.Configuration.Method === 'ParseSimple'}]
    }
    if (json.Configuration.Method.match(/ParseSimple.*/)) {
        json.Configuration.Method = 'ParseSimple'
    }  

    console.log(json.Configuration)
    return json
}

function reevaluateMatch(fieldName, regex, flags) {
    try {
        previewElement.textContent = new RegExp(regex, flags).test(previewData[fieldName]) + ' '
        previewElement.style.backgroundColor = '#f7f7f7'
    } catch(err) {
        previewElement.style.backgroundColor = '#f76666'
        previewElement.textContent = err.message
    }
}

function reevaluateReplace(fieldName, regex, flags, replace) {
    try {
        previewElement.textContent = previewData[fieldName].replace(new RegExp(regex, flags), replace) + ' '
        previewElement.style.backgroundColor = '#f7f7f7'
    } catch(err) {
        previewElement.style.backgroundColor = '#f76666'
        previewElement.textContent = err.message
    }
}

function reevaluateSplit(fieldName, regex, flags) {
    try {
        const matches = previewData[fieldName].match(new RegExp(regex, flags))
        if (matches.length === 0) { 
            previewElement.textContent = 'null'
        } else {
            previewElement.textContent = matches.join('\r\n')
        }
        previewElement.style.backgroundColor = '#f7f7f7'
    } catch(err) {
        previewElement.style.backgroundColor = '#f76666'
        previewElement.textContent = err.message
    }
}

function reevaluate(fieldName, method, regex, caseInsensitive, replace) {
    if (method === 'Match') {
        reevaluateMatch(fieldName, regex, caseInsensitive ? 'i' : '')
    } else if (method === `Replace`) {
        reevaluateReplace(fieldName, regex, caseInsensitive ? 'gi' : 'g', replace)
    } else if (method === 'ParseSimple') {
        reevaluateSplit(fieldName, regex, caseInsensitive ? 'gi' : 'g')
    } else {
        previewElement.style.backgroundColor = '#f76666'
        previewElement.textContent = `${method} preview not supported yet`
    }
}