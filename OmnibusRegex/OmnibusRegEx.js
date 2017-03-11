window.codeMirrorEditor = CodeMirror.fromTextArea(document.getElementById('regularExpression'), {
    mode: "regex"
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
        // .Add('ParseSimpleColumns', 'Split To Columns')
        .Add('ParseSimple', 'Split To Rows')
        // .Add('ParseComplex', 'Parse')
    methodSelectorItem.setValue(json.Configuration ? json.Configuration.Match : 'Match')
    manager.AddDataItem(methodSelectorItem)

    const matchFieldItem = new AlteryxDataItems.SimpleString({dataname: 'MatchField', id: 'MatchField'})
    matchFieldItem.setValue(json.Configuration && json.Configuration.Match ? json.Configuration.Match.Field : '')
    manager.AddDataItem(matchFieldItem)
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
    const showPreviewChanged = (v) => document.getElementById('preview').style.display = (v ? 'block' : 'none')
    showPreviewItem.BindUserDataChanged(showPreviewChanged)

    const fieldItem = manager.GetDataItem('Field')
    let data = {}
    const getFieldPreview = (v) => document.getElementById('preview').textContent = (v && data ? data[v] : ' ')
    plugInHelper.getInputDataArray('', 1, d => {
        data = d[0]
        getFieldPreview(fieldItem.value)
    })
    fieldItem.BindUserDataChanged(getFieldPreview)

    const methodItem = manager.GetDataItem('Method')
    const methodChanged = (v) => {
        document.getElementById('MatchFieldSet').style.display = (v === 'Match' ? 'block' : 'none')
    }
    methodItem.BindUserDataChanged(methodChanged)
    methodChanged(methodItem.value)

    const regExExpressionItem = manager.GetDataItem('RegExExpressionTemp')
    codeMirrorEditor.getDoc().setValue(regExExpressionItem.value)
    codeMirrorEditor.on('change', () => {
        regExExpressionItem.setValue(codeMirrorEditor.getDoc().getValue())
        console.log(regExExpressionItem.value)
    })
    regExExpressionItem.BindDataChanged((v) => console.log(v))
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

    const regexItem = manager.GetDataItem('RegExExpressionTemp')
    const regexText = regexItem.value

    let suffixText = ''
    if (method === `Replace`) {
        suffixText = `\n<Replace>`
    }

    return `${methodName}:\n${regexText.Length > 30 ? regexText.Substring(0, 27)+ '...' : regexText}${suffixText}`
}

/**
 * Reformat the JSON to the style we need
 */
Alteryx.Gui.BeforeGetConfiguration = function (json) {
    json.Configuration.CaseInsensitive = [{'@value': json.Configuration.CaseInsensitive}]
    json.Configuration.RegExExpression = [{'@value': json.Configuration.RegExExpressionTemp}]
    delete json.Configuration.RegExExpressionTemp

    json.Configuration.Match = {
        Field: json.Configuration.MatchField || `${json.Configuration.Field}_Matched`,
        ErrorUnmatched: [{'@value': json.Configuration.ErrorUnmatched}]
    }
    delete json.Configuration.MatchField
    delete json.Configuration.ErrorUnmatched

    console.log(json.Configuration)
    return json
}

function reevaluate() {
    const expression = `REGEX_Match([${document.getElementById('myFields').value}], "${document.getElementById('monkeys').value}")`
    plugInHelper.testExpression(expression, o => {
        if (o.error.errorMessage !== '') {
            document.getElementById('bananas').textContent = o.error.errorMessage
            return
        }

        plugInHelper.formulaPreview('', '__TEST__', 'Bool', expression, o => document.getElementById('bananas').textContent = o)
    })
}

document.addEventListener('keydown', (e) => {
    if (!e.key.match(/F1/)) {
        return
    }

    e.preventDefault() // prevent the normal help methods from being triggered
    e.stopPropagation()
    plugInHelper.openHelpPage('RegEx.htm')
})
