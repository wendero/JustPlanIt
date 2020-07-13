import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Room } from './components/Room';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route exact path='/room/:roomId' component={Home} />
        <Route exact path='/room/:roomId/:memberId' component={Room} />
      </Layout>
    );
  }
}
