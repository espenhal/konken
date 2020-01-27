import React, {Component} from 'react';

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

        fetch(`api/Data/league`)
            .then(handleErrors)
            .then(response => response.json())
            .then(data => {
                console.log(data)
                this.setState({league: data, loading: false});
            })
            .catch(error => console.error(error));
    }

    static renderLeague(league) {
        if (!league.Players) {
            return <div>tomt</div>;
        }

        return (
            <table className='table table-striped'>
                <thead>
                <tr>
                    <th>Navn</th>
                    <th>Lag</th>
                </tr>
                </thead>
                <tbody>
                {league.Players.map(player =>
                    <tr key={player.FplPlayerId}>
                        <td>{player.Name}</td>
                        <td>{player.TeamName}</td>
                    </tr>
                )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Home.renderLeague(this.state.league);

        return (
            <div>
                <h1>Alle oppdrag</h1>
                {contents}
            </div>
        );
    }
}
