/// <reference path="../AlteryxDesigner.d.ts" />
Alteryx.Gui.BeforeLoad = function (manager, dataItems) {
    if (!manager.GetDataItemByDataName('ColumnName')) {
        manager.AddDataItem(new dataItems.SimpleString({ dataname: 'ColumnName', id: 'ColumnName', initialValue: 'HelloFromJS' }));
    }
    if (!manager.GetDataItemByDataName('Value')) {
        manager.AddDataItem(new dataItems.SimpleString({ dataname: 'Value', id: 'Value', initialValue: 'Hello From JavaScript' }));
    }
};
Alteryx.Gui.AfterLoad = function (manager) {
    var ColumnNameItem = manager.GetDataItemByDataName('ColumnName');
    ColumnNameItem.BindUserDataChanged(function (newValue) {
        var newString = (newValue || '').toString();
        var element = document.getElementById('fieldNameValid');
        if (element)
            element.innerHTML = (newString.trim() === '') ? 'You must enter a field name' : '';
    });
};
Alteryx.Gui.Annotation = function (manager) {
    var ColumnNameItem = manager.GetDataItemByDataName('ColumnName');
    var ValueItem = manager.GetDataItemByDataName('Value');
    return (ColumnNameItem.getValue()) + ":\r\n" + ValueItem.getValue().replace(/^(.{27}).{3,}/, '$1...');
};
Alteryx.Gui.BeforeGetConfiguration = function (json) {
    if ((json.Configuration.ColumnName || '').trim() === '') {
        json.Configuration.ColumnName = 'NewJSColumn';
        json.Annotation = "NewJSColumn:\r\n" + (json.Annotation || '').replace(/^.*?:\r\n/, '');
    }
    return json;
};
