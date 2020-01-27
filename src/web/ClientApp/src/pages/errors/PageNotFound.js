import React, {Component} from 'react';

export class PageNotFound extends Component {
    render() {
        return (
            <div className={'container row mb-6'}>
                <div className={'col s12'}>
                    <div className={'text-center'}>
                        <h1 className={'large'}>404 - Siden ble ikke funnet</h1>
                    </div>
                </div>
            </div>
        );
    }
}
