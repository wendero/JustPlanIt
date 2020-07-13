import React, { Component } from 'react'
import { Row, Col, Card, CardHeader, CardBody, Input, ListGroup, ListGroupItem, Badge, Button, InputGroupAddon, InputGroup } from 'reactstrap'
import { Loading } from "./Loading"

export class Room extends Component {
  timer = null

  state = {
    data: null,
    roomId: 0,
    memberId: 0,
    leader: false,
    voting: false,
    adding: false,
    url: null,
  }

  componentWillMount = async () => {
    const { roomId, memberId } = this.props.match.params
    const url = `${window.location.href.replace(window.location.pathname, '')}/room/${roomId}`

    await this.setState({
      ...this.state,
      memberId: memberId,
      roomId: roomId,
      url: url,
    })

    this.loadData()
  }
  loadData = async () => {
    const { roomId, memberId, closing, results } = this.state
    if (closing) {
      clearInterval(this.timer)
      return
    }

    const response = await fetch(`api/room/${roomId}`)
    const data = await response.json()
    const leader = data.members.filter(f => f.leader)
    const votingStory = data.stories.filter(f => f.status == 2)
    const resultStory = data.stories.filter(f => f.status == 4)

    await this.setState({
      ...this.state,
      memberId: memberId,
      roomId: roomId,
      data: data,
      leader: leader ? leader[0].identifier : null,
      voting: votingStory != null ? votingStory[0] : false,
    })

    if (resultStory != null && resultStory.length > 0) {
      if (!results) {
        await this.showResults(resultStory[0].identifier)
      }
    }
    else {
      await this.setState({
        ...this.state,
        results: null,
      })
    }
  }

  showResults = async (storyId) => {
    const { roomId, results } = this.state

    if (results && results.story.identifier == storyId)
      return

    const url = `api/room/${roomId}/vote/${storyId}/results/`

    const response = await fetch(url)
    const result = await response.json()

    this.setState({
      ...this.state,
      results: result
    })
  }

  onAddNewStoryKeyDown = async (event) => {
    if (event.keyCode == 13) {
      const { roomId } = this.state
      const url = `api/room/${roomId}/story`
      const value = event.target.value

      event.target.value = ''

      const requestOptions = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Title: value })
      }

      await fetch(url, requestOptions)
    }
  }
  componentDidMount = () => {
    this.timer = setInterval(() => {
      this.loadData()
    }, 1000)
  }
  closeRoom = async () => {
    const { roomId } = this.state
    this.setState({
      ...this.state,
      closing: true
    })
    const url = `api/room/${roomId}`

    const requestOptions = {
      method: 'DELETE',
    }
    const response = await fetch(url, requestOptions)

    if (response.status == 200) {
      this.props.history.push('/')
    }
  }

  render = () => {
    const { data, leader, memberId, voting, results } = this.state
    const amILeader = leader == memberId

    return (
      <div>
        {!data ? <Loading /> : (
          <div>
            <h1>{data.name}</h1>
            {voting ? <h3 className="bg-info rounded p-2 color-white">{voting.title}</h3> : results ? <h3 className="bg-success rounded p-2 color-white">{results.story.title}</h3> : <h3 className="p-2">&nbsp;</h3>}
            <Row>
              <Col md={{ size: 9 }}>
                {!results ? (
                  <div>
                    {
                      data.deck.map((item, index) => {
                        return (
                          <ScrumCard key={index} state={this.state} item={item} index={index} />
                        )
                      })
                    }
                  </div>
                ) : (
                    <Results state={this.state} />
                  )}
                <div className="mt-3">
                  <h3>Stories</h3>
                  <ListGroup className="block">
                    {amILeader ? (
                      <ListGroupItem>
                        <Row>
                          <Col>
                            <Input placeholder="add new story" onKeyDown={this.onAddNewStoryKeyDown}></Input>
                          </Col>
                        </Row>
                      </ListGroupItem>
                    ) : null}
                    {
                      data.stories.map((item, index) => {
                        return (
                          <Story key={index} state={this.state} item={item} />
                        )
                      })
                    }
                  </ListGroup>
                </div>
              </Col>
              <Col md={{ size: 3 }}>
                <Team state={this.state} />
                <Card className="no-border shadow mt-3">
                  <CardBody className="no-border">
                    <h5>Invite your teammates</h5>
                    <Input readOnly value={this.state.url} />
                  </CardBody>
                </Card>

                {amILeader ? (
                  <Card className="no-border shadow mt-3">
                    <CardBody className="no-border">
                      <Button color="danger" className="btn-block" onClick={this.closeRoom}>Close room</Button>
                    </CardBody>
                  </Card>
                ) : (null)}
              </Col>
            </Row>
          </div>
        )
        }
      </div>
    )
  }
}

class Results extends React.Component {
  componentWillMount = () => {
    this.setState(this.props.state)
  }

  componentWillReceiveProps = () => {
    this.setState(this.props.state)
  }

  pointsChanged = (event) => {
    const points = event.target.value

    this.setState({
      ...this.state,
      points: points
    })
  }

  setPoints = async (storyId, event) => {
    const { roomId, points } = this.state
    const url = `api/room/${roomId}/story/${storyId}`

    const requestOptions = {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        Points: Number(points),
        Status: 3
      })
    }
    await fetch(url, requestOptions)
  }

  render = () => {
    const { results, memberId, leader } = this.state
    const amILeader = leader == memberId
    const points = results && results.story.points > 0 ? results.story.points : ''

    return (
      <div>
        {!results ? null : (
          <div>
            <Row>
              <Col className="text-center d-flex align-items-stretch">
                <Card className="full-width">
                  <CardHeader className="back-green-important bold">
                    Average
                  </CardHeader>
                  <CardBody className="h-100">
                    <div className="font-5x bold">{results.average}</div>
                  </CardBody>
                </Card>
              </Col>
              <Col className="text-center d-flex align-items-stretch">
                <Card className="full-width">
                  <CardHeader className="back-yellow-important bold">Maximum</CardHeader>
                  <CardBody>
                    <div className="font-5x bold">{results.maximum}</div>
                    {results.maximumMembers.map((item, index) => {
                      return (
                        <Badge key={index} color="primary" className="mr-1">{item.name}</Badge>
                      )
                    })}
                  </CardBody>
                </Card>
              </Col>
              <Col className="text-center d-flex align-items-stretch">
                <Card className="full-width">
                  <CardHeader className="back-info-important bold">Minimum</CardHeader>
                  <CardBody>
                    <div className="font-5x bold">{results.minimum}</div>
                    {results.minimumMembers.map((item, index) => {
                      return (
                        <Badge key={index} color="primary" className="mr-1">{item.name}</Badge>
                      )
                    })}
                  </CardBody>
                </Card>
              </Col>
            </Row>
            {amILeader ? (
              <Row className="mt-3">
                <Col>
                  <InputGroup>
                    <Input defaultValue={points} onChange={this.pointsChanged} placeholder="How many points for this story?" />
                    <InputGroupAddon addonType="append"><Button color="primary" onClick={this.setPoints.bind(this, results.story.identifier)}>Set</Button></InputGroupAddon>
                  </InputGroup>
                </Col>
                <Col>
                </Col>
              </Row>) : (
                null
              )}
          </div>
        )}
      </div>
    )
  }
}

class ScrumCard extends React.Component {

  vote = async (points) => {
    const { roomId, memberId } = this.state
    const storyId = this.state.voting.identifier
    const url = `api/room/${roomId}/vote/${storyId}`

    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ MemberId: Number(memberId), Points: points })
    }

    fetch(url, requestOptions)
  }

  componentWillMount = () => {
    this.setState(this.props.state)
  }

  componentWillReceiveProps = () => {
    this.setState(this.props.state)
  }

  render = () => {
    const { item } = this.props
    const { voting, memberId } = this.state

    return (
      <div className={`scrumCard shadow ${voting && voting.votes[memberId] && voting.votes[memberId] == item ? 'back-blue-important' : 'back-white'} rounded ${voting ? 'voting hover-teal-important' : 'disabled'}`} onClick={this.vote.bind(this, item)}>
        <div className="border-clouds border-1 no-margin no-padding full-height rounded">{item}</div>
      </div>
    )
  }
}
class Team extends React.Component {

  componentWillMount = () => {
    this.setState(this.props.state)
  }

  componentWillReceiveProps = () => {
    this.setState(this.props.state)
  }

  finishVoting = async () => {
    const { roomId } = this.state
    const url = `api/room/${roomId}/vote/stop`

    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    }
    const response = await fetch(url, requestOptions)
    const results = await response.json()

    this.setState({
      ...this.state,
      results: results,
      points: null
    })
  }

  cancelVoting = (storyId, status) => {
    const { roomId } = this.state
    const url = `api/room/${roomId}/story/${storyId}`

    const requestOptions = {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status: status })
    }
    fetch(url, requestOptions)
  }

  render = () => {
    const { data, leader, memberId, voting } = this.state
    const amILeader = leader == memberId
    const quantityOfVotes = voting ? Object.keys(voting.votes).length : 0

    return (
      <Card className="no-border shadow">
        <CardHeader className="border-none h3 back-blue-important">
          Team
        </CardHeader>
        <CardBody className="no-border no-padding">
          <ListGroup>
            {
              data.members.map((item, index) => {
                return (
                  <ListGroupItem key={item.identifier} className="" style={{ borderBottom: '1px solid lightgray' }}>
                    <h6>{item.name}</h6>
                    {leader == item.identifier ? <Badge color="danger" className="mr-1">leader</Badge> : (null)}
                    {memberId == item.identifier ? <Badge color="primary" className="mr-1">you</Badge> : (null)}
                    {voting && (!voting.votes || !voting.votes[`${item.identifier}`]) ? <Badge color="warning">waiting</Badge> : (null)}
                    {voting && voting.votes && voting.votes[`${item.identifier}`] ? <Badge color="success">voted</Badge> : (null)}
                  </ListGroupItem>
                )
              })
            }
          </ListGroup>
          {amILeader ? (
            <div className="p-3">
              <Button className="btn btn-block" color="success" disabled={!voting || !quantityOfVotes} onClick={this.finishVoting}>Finish Voting</Button>
              <Button className="btn btn-block" color="danger" outline disabled={!voting} onClick={this.cancelVoting.bind(this, voting ? voting.identifier : null, 0)}>Cancel Voting</Button>
            </div>
          ) : (
              null
            )}
        </CardBody>
      </Card>
    )
  }
}
class Story extends React.Component {

  componentWillMount = () => {
    this.setState(this.props.state)
  }

  componentWillReceiveProps = () => {
    this.setState(this.props.state)
  }

  getStatus = (status) => {
    switch (status) {
      case 0:
        return { label: "Ready", color: "primary" }
      case 2:
        return { label: "Voting", color: "warning" }
      case 3:
      case 4:
        return { label: "Voted", color: "success" }
      case 9:
        return { label: "Closed", color: "secondary" }
    }
  }

  changeStatus = (storyId, status) => {
    const { roomId } = this.state
    const url = `api/room/${roomId}/story/${storyId}`

    const requestOptions = {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status: status })
    }
    fetch(url, requestOptions)
  }

  deleteStory = (storyId) => {
    const { roomId } = this.state
    const url = `api/room/${roomId}/story/${storyId}`

    const requestOptions = {
      method: 'DELETE',
    }
    fetch(url, requestOptions)
  }

  startVoting = async (storyId) => {
    const { roomId } = this.state
    const url = `api/room/${roomId}/vote/${storyId}/start`

    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    }
    fetch(url, requestOptions)
  }

  showResults = async (storyId) => {
    const { roomId, results } = this.state

    if (results && results.story.identifier == storyId)
      return

    const url = `api/room/${roomId}/vote/${storyId}/results`

    const response = await fetch(url)
    const result = await response.json()

    this.setState({
      ...this.state,
      results: result,
      points: result.story.points ? result.story.points : null
    })
  }

  render = () => {
    const { item } = this.props
    const { leader, memberId, voting, results } = this.state
    const amILeader = leader == memberId

    return (
      <ListGroupItem key={item.identifier} className="" style={{ borderBottom: '1px solid lightgray' }}>
        <Row>
          <Col md={{ size: amILeader ? 8 : 12 }}>
            {item.status != 9 ? (
              <div className="h6">
                {item.title}
              </div>
            ) : (
                <del className="font-italic">{item.title}</del>
              )}
            <div>
              <Badge color="info" className="mr-1">{item.points} pts</Badge>
              <Badge color={this.getStatus(item.status).color} className="mr-1">{this.getStatus(item.status).label}</Badge>
            </div>
          </Col>
          {amILeader ? (
            <Col md="4" className="text-right">
              {item.status != 9 && item.status != 2 && !voting && !results && item.status != 3 && item.status != 4 ? <Button size="sm" outline color="primary" onClick={this.startVoting.bind(this, item.identifier)}>Vote</Button> : null}
              {item.status == 3 && !voting ? <Button size="sm" outline color="warning" onClick={this.showResults.bind(this, item.identifier)}>Results</Button> : null}
              {item.status != 9 && !(voting && voting.identifier == item.identifier) ? <Button size="sm" outline color="secondary" className="ml-1" onClick={this.changeStatus.bind(this, item.identifier, 9)}>Close</Button> : null}
              {item.status == 9 && !(voting && voting.identifier == item.identifier) ? <Button size="sm" outline color="success" className="ml-1" onClick={this.changeStatus.bind(this, item.identifier, item.points > 0 ? 3 : 0)}>Reopen</Button> : null}
              {!(voting && voting.identifier == item.identifier) ? <Button size="sm" outline color="danger" className="ml-1" onClick={this.deleteStory.bind(this, item.identifier)}>Delete</Button> : null}
            </Col>
          ) : (null)}
        </Row>
      </ListGroupItem>
    )
  }
}