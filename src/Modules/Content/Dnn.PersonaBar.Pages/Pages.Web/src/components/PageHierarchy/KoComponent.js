import {Component} from "react";
import ReactDOM from "react-dom";
/// <reference path="@types/knockout" />

const ko = parent.ko;

export default class KoComponent extends Component {
    componentDidMount() {
        this.node = ReactDOM.findDOMNode(this);
        this.__koTrigger = ko.observable(true);
        this.__koModel = ko.computed(function () {
            this.__koTrigger(); // subscribe to changes of this...
            return {
                props: this.props,
                state: this.state
            };
        }, this);

        ko.applyBindings(this.__koModel, this.node);
    }

    componentWillUnmount() {
        ko.cleanNode(this.node);
    }

    updateKnockout() {        
        this.__koTrigger(!this.__koTrigger());
    }

    componentDidUpdate() {
        this.updateKnockout();
    }
}