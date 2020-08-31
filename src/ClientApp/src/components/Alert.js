import React, { Component } from 'react';
import { UncontrolledAlert } from 'reactstrap';

export class Alert extends Component {
    render() {
        const { alert } = this.props
        return (
            <UncontrolledAlert color={ alert.color ? alert.color : 'danger' }>
                { alert.message }
            </UncontrolledAlert>
        )
    }
}

