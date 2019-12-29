import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);
    this.state = {league: {}, loading: true};

    function handleErrors(response) {
      if (!response.ok) {
        throw Error(response.statusText);
      }
      return response;
    }

    fetch(`api/League`)
        .then(handleErrors)
        .then(response => response.json())
        .then(data => {
          this.setState({league: data, loading: false});
        })
        .catch(error => console.error(error));
  }
  
  render () {
    return (
      <div>
        test
      </div>
    );
  }
}
