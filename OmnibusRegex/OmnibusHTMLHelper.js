window.PlugInHelper = (() => { return { Create: (alteryx, manager, window) => {
    const metaInfo = manager.metaInfo
    const formulaConstants = []

    const jsEvent = (obj) => alteryx.JsEvent(JSON.stringify(obj))
    const openHelpPage = (url) => jsEvent({Event: 'OpenHelpPage', url})

    let callbackId = 0;
    function makeCallback(callback) {
        const callbackName = `callback${callbackId++}`

        window[callbackName] = (o) => {
            delete window[callbackName]
            callback(o)
        }

        return callbackName
    }

    // Get Formula Constants 
    let formulaConstantsCallback = () => {}
    jsEvent({Event: 'GetFormulaConstants', callback: makeCallback((obj) => {
        formulaConstants.push(...obj.formulaConstants.map((c) => { return { name: c.FullName, isNumeric: c.IsNumeric, value: c.Value } }))
        formulaConstants.push(...obj.questionConstants.map((c) => { return { name: c.FullName, isNumeric: c.IsNumeric, value: c.Value } }))
        formulaConstantsCallback()
    })})

    // Get Connections
    const connectionNames = Array(metaInfo.Count).fill().map((_, i) => metaInfo.Get(i).ConnectionName)
    const connections = {}
    connectionNames.forEach((n, i) => {
        const fields = metaInfo.Get(i)._GetFields().map((f, j) => { return { name: f.strName, type: f.strType, size: f.nSize, scale: f.nScale, description: f.strDescription, index: j } })
        connections[n] = fields
    })

    // Read Input Data
    function getInputData(connection, rows, callback) {
        jsEvent({
            Event: 'GetInputData',
            callback: makeCallback(callback),
            anchorIndex: 0,
            connectionName: connection,
            numRecs: rows,
            offset: 0})
    }

    function getInputDataArray(connection, rows, callback) {
        const newCallback = (o) => {
            const newObj = o.data ? o.data.map((d) => {
                const output = {}
                d.forEach((v, i) => output[o.fields[i].strName] = v)
                return output
            }) : {}
            callback(newObj)
        }
        getInputData(connection, rows, newCallback)
    }

    // Test Expression
    function testExpression(expression, callback) {
        jsEvent({
            Event: 'TestExpression',
            callback: makeCallback(callback),
            expression,
            customFields: []
        })
    }

    // Formula Preview
    function formulaPreview(connection, name, type, expression, callback) {
        const previewObject = [{name, type, expression}]

        jsEvent({
	      Event: 'FormulaPreview',
	      connectionName: connection,
	      anchorIndex: 0,
	      callback: makeCallback(callback),
	      expressions: previewObject
	    })

    }

    return {
        jsEvent,
        openHelpPage,
        formulaConstants,
        connectionNames,
        connections,
        getInputData,
        getInputDataArray,
        setFormulaConstantsCallback: (callback) => formulaConstantsCallback = callback,
        testExpression,
        formulaPreview
    }
} } })()