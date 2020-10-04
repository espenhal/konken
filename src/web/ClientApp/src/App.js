import React, { Component } from 'react';
import {Home} from "./pages/Home";
import {PageNotFound} from "./pages/errors/PageNotFound";
import {Switch, Route} from 'react-router-dom';
import './css/app.css';
import M from 'materialize-css';
M.AutoInit();

export default class App extends Component {
  constructor(props) {
    super(props);

    this.state = {};
  }

  componentDidMount() {
    document.body.classList.add(this.state.theme);
  }

  componentWillUnmount() {
    let body = document.body;
    body.classList.remove(...body.classList);
  }

  render() {
    return (
        <Switch>
          <Route exact path="/" title={'Home'} key={'home'}>
            <Home
                globalState={this.state}
            />
          </Route>
          <Route component={PageNotFound}/>
        </Switch>
    );
  }
}
