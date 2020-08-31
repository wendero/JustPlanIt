import React, { Component } from 'react';
import { Row, Col, Card, CardHeader, CardBody, Input, Button } from 'reactstrap';
import { Redirect } from 'react-router-dom'
import { Loading } from './Loading'
import { Alert } from './Alert';

export class Home extends Component {
  static displayName = Home.name;


  state = {
    room: '',
    roomId: null,
    name: '',
    redirect: false,
    data: null,
    loaded: null,
    alert: null
  }

  OnRoomChange = (event) => {
    this.setState({
      ...this.state,
      room: event.target.value
    })
  }
  OnNameChange = (event) => {
    this.setState({
      ...this.state,
      name: event.target.value
    })
  }

  CreateRoom = async (event) => {
    try {
      const url = `api/room/`

      const requestOptions = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ room: this.state.room, name: this.state.name })
      };

      const response = await fetch(url, requestOptions);
      const data = await response.json();

      await this.setState({
        ...this.state,
        data: data,
      })
      const roomId = data.identifier;
      const memberId = data.members[0].identifier;

      this.setState({
        ...this.state,
        redirect: `/room/${roomId}/${memberId}`
      });
    }
    catch {
      this.setState({
        ...this.state,
        alert: { color: 'danger', message: 'Something went wrong... The service seems to be offline... =(' }
      })
    }
  }
  JoinRoom = async (event) => {
    try {
      const { roomId, name } = this.state
      const url = `api/room/${roomId}/join`

      const requestOptions = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: name })
      };

      const response = await fetch(url, requestOptions);
      const data = await response.json();
      if (response.status == 404) {
        var message = data.message
        if (!message) {
          message = "Joining failed"
        }

        this.setState({
          ...this.state,
          alert: {
            message: message,
            color: 'danger'
          }
        })
      }

      const memberId = data.identifier;

      this.setState({
        ...this.state,
        redirect: `/room/${roomId}/${memberId}`
      });
    }
    catch {
      this.setState({
        ...this.state,
        alert: { color: 'danger', message: 'Something went wrong... The service seems to be offline... =(' }
      })
    }
  }
  CheckCreateButton = () => {
    return this.state.name == '' || this.state.room == '';
  }

  RenderRedirect = () => {
    if (this.state.redirect) {
      return <Redirect to={{ pathname: this.state.redirect, data: this.state.data }} />
    }
  }
  componentWillMount = async () => {
    const { roomId } = this.props.match.params
    if (roomId) {
      const response = await fetch(`api/room/${roomId}`);
      if (response.status == 200) {
        const data = await response.json();
        this.setState({
          ...this.state,
          roomId: roomId,
          room: data.name,
        })
      }
      else {
        this.setState({
          ...this.state,
          alert: {
            message: 'Room not found... maybe it no longer exists...',
            color: 'warning'
          }
        })
      }
    }
  }

  componentDidMount = () => {
    this.setState({
      ...this.state,
      loaded: true
    })
  }

  render() {
    const { loaded, alert } = this.state
    return (
      <div>
        {!loaded ? <Loading /> : (
          <Row>
            {alert ? <Alert alert={alert} /> : null}
            {this.RenderRedirect()}
            <Col>
              <Card>
                <CardHeader>{this.state.roomId ? 'Join session' : 'New session'}</CardHeader>
                <CardBody>
                  {this.state.roomId ? (
                    <h5>{this.state.room}</h5>
                  ) : (
                      <Input type="text" value={this.state.room} placeholder="room name" onChange={this.OnRoomChange}></Input>
                    )}
                  <Input type="text" value={this.state.name} placeholder="your name" className="mt-2 mb-2" onChange={this.OnNameChange}></Input>
                  {this.state.roomId ? (
                    <Button className="btn btn-primary btn-block" disabled={this.CheckCreateButton()} onClick={this.JoinRoom}>Join</Button>
                  ) : (
                      <Button className="btn btn-primary btn-block" disabled={this.CheckCreateButton()} onClick={this.CreateRoom}>Create</Button>
                    )}
                </CardBody>
              </Card>
            </Col>
            <Col></Col>
          </Row>
        )}
      </div>
    );
  }
}
