class Header extends React.Component {
    render() {
        const { alarm, latch } = this.props
        return (
            <div class="row m-0 p-0">
                <div class="col m-0 p-0">
                    <span class="h1 mr-1">{alarm && latch ? (<Alarm class="mr-1" alarm={alarm} latch={latch} />) : null}{this.props.title || "<undefined>"}</span>
                    {this.props.children}
                </div>
            </div>
        )
    }
}
class Time extends React.Component {
    render() {
        return (
            <div class={`font-0_8x badge badge-light not-bold ${this.props.class}`}><i class="far fa-clock"></i> <span>{this.props.time}</span></div>
        )
    }
}
class FillOver extends React.Component {
    render() {
        return (
            <div class="fillover"><div>{this.props.children}</div></div>
        )
    }
}
class Loading extends React.Component {
    render() {
        return (
            <FillOver><div class="box"><i class="fal fa-spinner font-3x fa-spin"></i>{this.props.children}</div></FillOver>
        )
    }
}
class Alerts extends React.Component {
    render() {
        const { alerts, children } = this.props

        return (
            <div class="alerts">
                {children}
                {
                    alerts.map((alert) => (
                        <Alert key={alert.id} onDestroy={this.destroyAlert} alert={alert} />
                    ))
                }
            </div>
        )
    }
}
class Alarm extends React.Component {
    render() {
        const { alarm, latch, onClick } = this.props
        return (
            <span onClick={onClick ? onClick : null} class={this.props.class}><i class={`fa fa-exclamation-circle ${latch}`}></i><i class={`fa fa-circle ${alarm}`}></i></span>
        )
    }
}

class Alert extends React.Component {
    state = {
        class: '',
        hide: false,
        shown: false
    }
    show(duration) {
        setTimeout(() => {
            this.setState({
                ...this.state,
                class: 'show',
                shown: true
            })

            setTimeout(() => {
                this.hide()
            }, duration)

        }, 100);
    }
    hide = () => {
        if (this.state.hide || this.state.class == 'hiding')
            return;

        this.setState({
            ...this.state,
            class: 'hiding'
        })

        setTimeout(() => {
            this.props.alert.status = 'deleted'
            this.setState({
                ...this.state,
                hide: true
            })
        }, 500)
    }
    render() {
        const duration = this.props.duration ? this.props.duration : 5000;
        const { hide, shown } = this.state
        const { alert, children } = this.props;

        if (!shown)
            this.show(duration)
        if (hide) {
            return null;
        }

        const message = children ? children : alert ? alert.message : null;
        const alertType = alert && alert.type ? alert.type : 'info';

        return (
            <div onClick={this.hide} class={`alert back-${alertType} ${this.state.class}`} role="alert">{message}</div>
        )
    }
}
class Parameter extends React.Component {
    state = {
        showDetails: false
    }
    getParameter() {
        const { item, labels, config, type, metadata, defaultMetadata } = this.props;
        const propertyName = this.props.children
        let propertyMetadata = GetProperty(metadata, propertyName)

        if (defaultMetadata) {
            propertyMetadata = { ...propertyMetadata, ...defaultMetadata }
        }

        const label = GetProperty(labels, propertyName, true)
        const rawValue = GetProperty(item, propertyName)
        const valueRpn = propertyMetadata && propertyMetadata.Value ? GetProperty(metadata, propertyName + ".Value") : null

        let value = valueRpn ? RPN.Eval(valueRpn, [rawValue, item]) : rawValue
        if (value && typeof value == 'object' && value.Value) {
            value = value.Value
        }

        const alarmItem = GetProperty(item.Alarms, propertyName)

        const alarm = !(propertyMetadata && propertyMetadata.Alarm) ? { color: 'color-disabled' } : {
            color: RPN.Eval(propertyMetadata.Alarm.Color, [alarmItem.AlarmLevel, item]),
            back: RPN.Eval(propertyMetadata.Alarm.Back, [alarmItem.AlarmLevel, item]),
            value: RPN.Eval(propertyMetadata.Alarm.Value, [alarmItem.AlarmValue, item]),
            at: new Date(GetProperty(alarmItem, "AlarmAt")).ToTanderaFormat(config.DateFormat)
        }
        const latch = !(propertyMetadata && propertyMetadata.Latch) ? { color: 'color-disabled' } : {
            color: RPN.Eval(propertyMetadata.Latch.Color, [alarmItem.LatchLevel, item]),
            back: RPN.Eval(propertyMetadata.Latch.Back, [alarmItem.LatchLevel, item]),
            value: RPN.Eval(propertyMetadata.Latch.Value, [alarmItem.LatchValue, item]),
            at: new Date(GetProperty(alarmItem, "LatchAt")).ToTanderaFormat(config.DateFormat)
        }

        if (alarm && !alarm.value) alarm.value = GetProperty(item, `Alarms.${propertyName}.AlarmValue`);
        if (alarm && !alarm.at) alarm.value = GetProperty(item, `Alarms.${propertyName}.AlarmAt`);
        if (latch && !latch.value) latch.value = GetProperty(item, `Alarms.${propertyName}.LatchValue`);
        if (latch && !latch.at) latch.value = GetProperty(item, `Alarms.${propertyName}.LatchAt`);

        // if (alarm.value) alarm.value = GetProperty(item, `Alarms.${propertyName}.AlarmValue`);

        switch (type) {
            case "dateTime":
                value = new Date(value).ToTanderaFormat(config.DateFormat)
                break;
            default:
                break;
        }

        return {
            hasDetails: alarm && alarm.value,
            name: propertyName,
            metadata: propertyMetadata,
            label: label,
            raw: rawValue,
            value: value,
            alarm: alarm,
            latch: latch,
            item: item
        }
    }
    render() {
        const parameter = this.getParameter()
        const { latch, alarm, value, label, hasDetails } = parameter
        const { showDetails } = this.state

        const displayType = this.props.displayType || "all"

        if (displayType == 'block') {
            return (
                <Block parameter={parameter} />                
            )
        }

        return (
            <div class={`parameter`}>
                <Alarm onClick={() => { hasDetails ? this.setState({ showDetails: !this.state.showDetails }) : null }} class={`mr-1`} alarm={alarm.color} latch={latch.color} />
                <span>
                    {['all', 'label'].includes(displayType) ? (
                        <span class="bold">{label}</span>
                    ) : null}
                    {['all'].includes(displayType) ? (
                        <span class="mr-1">:</span>
                    ) : null}
                    {['all', 'value'].includes(displayType) ? (
                        <span>{value}</span>
                    ) : null}
                </span>
                {showDetails ? (
                    <div class="alarm-detail p-1">
                        <div>
                            <span class={`badge ${alarm.back}`}>alarm</span>
                            <span class="badge badge-light">{alarm.value} at {alarm.at}</span>
                        </div>
                        <div>
                            <span class={`badge ${latch.back}`}>latch</span>
                            <span class="badge badge-light">{latch.value} at {latch.at}</span>
                        </div>
                    </div>
                ) : null}
            </div>
        )
    }
}

class Block extends React.Component {
    state = {
        showDetails: false
    }
    render() {
        const parameter = this.props.parameter
        const { latch, alarm, label } = parameter
        const { showDetails } = this.state

        return (
            <div class="alarmBlock">
                <div class={`alarm ${alarm.back} ${latch.back}`} onClick={() => { this.setState({ showDetails: !this.state.showDetails }) }}>
                    <div>{label}</div>
                </div>
                {showDetails ? (
                    <div class="alarm-detail mt-1">
                        <div>
                            <span class={`badge ${alarm.back}`}>alarm</span>
                            <span class="badge badge-light">{alarm.at}</span>
                        </div>
                        <div>
                            <span class={`badge ${latch.back}`}>latch</span>
                            <span class="badge badge-light">{latch.at}</span>
                        </div>
                    </div>
                ) : null}
            </div>

        )
    }
}

class Message {
    constructor(message, type) {
        this.id = NewID()
        this.message = message
        this.type = type
    }
}

function GetSystemAlerts(alerts, systemAlerts) {
    var uiAlerts = [];

    if (alerts && alerts.length) {
        systemAlerts = alerts.map(item => {
            if (systemAlerts.includes(item.ID))
                return item.ID;

            const alert = new Message(item.Message, alerts[0].AlertType == 'Error' ? 'danger' : alerts[0].AlertType.toLowerCase());
            systemAlerts.push(item.ID);
            uiAlerts.push(alert);
            return item.ID;
        })
    }
    return uiAlerts;
}