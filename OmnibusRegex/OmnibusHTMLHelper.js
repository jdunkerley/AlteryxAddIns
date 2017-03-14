window.PlugInHelper = (() => { return { Create: (alteryx, manager, AlteryxDataItems, window) => {
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
    const connectionNames = Array(manager.metaInfo.Count).fill().map((_, i) => manager.metaInfo.Get(i).ConnectionName)
    const connections = {}
    connectionNames.forEach((n, i) => {
        const fields = manager.metaInfo.Get(i)._GetFields().map((f, j) => { return { name: f.strName, type: f.strType, size: f.nSize, scale: f.nScale, description: f.strDescription, index: j } })
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

    function truncateString(input, length = 30) {
        return input.length > length ? `${input.substring(0, 27)}...` : input
    }

    function createDataItem(dataName, value, suppressed = false) {
        let output
        switch (typeof(value)) {
            case "boolean":
                output = new AlteryxDataItems.SimpleBool({dataname: dataName, id: dataName})
                break
            case "string":
                output = value.match(/^\d{4}-\d{2}-\d{2}$/) 
                    ? new AlteryxDataItems.SimpleDate({dataname: dataName, id: dataName})
                    : new AlteryxDataItems.SimpleString({dataname: dataName, id: dataName})
                break
            case "number":
                output = value % 1
                    ? new AlteryxDataItems.SimpleFloat({dataname: dataName, id: dataName})
                    : new AlteryxDataItems.SimpleInt({dataname: dataName, id: dataName})
                break;
        }

        output.suppressed = suppressed
        output.setValue(value)
        manager.AddDataItem(output)
        return output
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
        formulaPreview,
        truncateString,
        createDataItem
    }
} } })()