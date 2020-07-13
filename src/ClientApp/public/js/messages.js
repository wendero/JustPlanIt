var currentCulture ="en_US";
var languages = {
    en_US: {
        connectionlost: "Connection lost", 
        elementlost: "Element is not listening",
        agentlost: "Agent is not listening",
        refreshOn: "Refresh is enabled",
        refreshOff: "Refresh is disabled",
        latchResetOK: "Latches has been reset",
        latchResetError: "Error while resetting latches",
        singleLatchResetOK: "Latch has been reset for {0}",
        singleLatchResetError: "Error while resetting latch",
        errorRetrievingData: "Error while retrieving data",
        errorExecutingAction: "Error while executing action"
    }
};
var language = languages[currentCulture];
var messages = {
    overall: {
        general: {
            id: "overallgeneral",
            obj: $('<div><i class="fa fa-exclamation-triangle fa-2x"></i></div>')
        },
        connectionlost: {
            id: "connectionlost",
            obj: $('<div><div><i class="fa fa-unlink fa-2x"></i></div><div>' + language.connectionlost + '</div></div>')
        },
        elementlost: {
            id: "elementlost",
            obj: $('<div><div><i class="fa fa-deaf fa-2x"></i></div><div>' + language.elementlost + '</div></div>')
        },
        agentlost: {
            id: "agentlost",
            obj: $('<div><div><i class="fa fa-deaf fa-2x"></i></div><div>' + language.agentlost + '</div></div>')
        }
    },
    alerts: {
        refreshOn: language.refreshOn,
        refreshOff: language.refreshOff,
        latchResetOK: language.latchResetOK,
        latchResetError: language.latchResetError,
        singleLatchResetOK: language.singleLatchResetOK,
        singleLatchResetError: language.singleLatchResetError,
        errorRetrievingData: language.errorRetrievingData,
        errorExecutingAction: language.errorExecutingAction
    }
};