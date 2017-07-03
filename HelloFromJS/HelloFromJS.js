/// <reference path="../AlteryxEngine.d.ts" />
var globalConfiguration = {
    state: '',
    columnName: 'HelloFromJS',
    value: 'Hello from JavaScript'
};
Alteryx.Plugin.DefineConnections = function () {
    console.log('DefineConnections Called.');
    return {
        IncomingConnections: [{ type: 'Input' }],
        OutgoingConnections: [{ name: 'Output' }]
    };
};
Alteryx.Plugin.PI_Init = function (config) {
    console.log('PI_Init Called.');
    globalConfiguration.state = 'Inited';
    if (config.Configuration) {
        globalConfiguration.columnName = config.Configuration['ColumnName'] || globalConfiguration.columnName;
        globalConfiguration.value = config.Configuration['Value'] || globalConfiguration.value;
    }
};
Alteryx.Plugin.II_Init = function (metaInfo) {
    console.log("II_Init Called: " + metaInfo.Connection + ".");
    globalConfiguration.state = 'Connected';
    var newField = { name: globalConfiguration.columnName, type: 'V_WString', size: 64532 };
    Alteryx.Engine.SendMessage.RecordInfo('Output', { Field: metaInfo.RecordInfo.Field.concat([newField]) });
};
Alteryx.Plugin.II_PushRecords = function (data) {
    console.log("II_PushRecords Called: " + data.Connection + ".");
    Alteryx.Engine.SendMessage.PushRecords('Output', data.Records.map(function (r) { return r.concat([globalConfiguration.value]); }));
    Alteryx.JsEvent(JSON.stringify({ Event: 'PushRecords', Connection: 'Output', ToolProgress: data.Progress, Records: '[]' }));
};
Alteryx.Plugin.II_AllClosed = function () {
    console.log('II_AllClosed Called.');
    globalConfiguration.state = 'Closed.';
    Alteryx.Engine.SendMessage.CloseOutput('Output');
    Alteryx.Engine.SendMessage.Complete();
};
Alteryx.Plugin.PI_Close = function () {
    console.log('PI_Close Called.');
    if (globalConfiguration.state === 'Inited') {
        Alteryx.Engine.SendMessage.Error('HelloFromJS requires an Input connection');
    }
    Alteryx.Engine.SendMessage.PI_Close();
};
