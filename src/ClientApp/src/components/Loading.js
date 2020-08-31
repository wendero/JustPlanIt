import React, { Component } from 'react';

class FillOver extends Component {
    render() {
        return (
            <div className="fillover"><div>{this.props.children}</div></div>
        )
    }
}
export class Loading extends Component {
    render() {
        return (
            <FillOver><div className="box"><i className="fal fa-spinner font-3x fa-spin"></i>{this.props.children}</div></FillOver>
        )
    }
}