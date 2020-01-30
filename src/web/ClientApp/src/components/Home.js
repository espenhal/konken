import React, {Component} from 'react';

export class Home extends Component {
    static displayName = Home.name;

    constructor(props) {
        super(props);
        this.state = {players: {}, loading: true};

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
                this.setState({players: data, loading: false});
            })
            .catch(error => console.error(error));
    }

    static renderLeague(players) {
        if (!players) {
            return <div>tomt</div>;
        }

        return (
            <table className='table table-striped'>
                <thead>
                <tr>
                    <th>Lag</th>
                    <th className='points'>KONKEN</th>
                    <th>$$$Gevinst</th>
                    <th>Rundeseire</th>
                    <th>Cup</th>
                    <th>$</th>
                    <th>Poing</th>
                    <th>Benkpoing</th>
                    <th>Bytter</th>
                    <th>Byttekost</th>                    
                    <th>Chips</th>
                </tr>
                </thead>
                <tbody>
                {players.map(player =>
                    <tr key={player.FplPlayerId}>
                        <td>{player.TeamName} ({player.Name})</td>
                        <td className='points'>{player.PointsTransferCostsExcluded}</td>                        
                        <td>{player.Cash}</td>
                        <td>{player.GameweeksWon}</td>
                        <td>{player.CupRounds}</td>
                        <td>{player.Value}</td>
                        <td>{player.Points}</td>
                        <td>{player.PointsOnBench}</td>
                        <td>{player.Transfers}</td>
                        <td>{player.TransferCosts}</td>
                        <td>{player.Chips}</td>
                    </tr>
                )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Home.renderLeague(this.state.players);

        return (
            <div>
                <h1>FPL konken 19/20</h1>
                {contents}
            </div>
        );
    }
}
