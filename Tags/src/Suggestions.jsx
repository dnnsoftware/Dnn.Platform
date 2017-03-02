import React, { Component, PropTypes } from "react";
import {Scrollbars} from "react-custom-scrollbars";

const style = {
    list: {
        height: 170,
        border: "1px solid #E3E3E3",
        backgroundColor: "#FFFFFF"
    }
};

export default class Suggestions extends Component {
    onSelectSuggestion(tag) {
        if (typeof (this.props.onSelectSuggestion) === "function") {
            this.props.onSelectSuggestion(tag);    
        }
    }

    onScrollUpdate(scroll) {
        if (typeof (this.props.onScrollUpdate) === "function") {
            this.props.onScrollUpdate(scroll);    
        }
    }

    getSuggestions() {
        return this.props.suggestions.map((suggestion, index) => {
            return <div className="suggestion" key={index} onClick={this.onSelectSuggestion.bind(this, suggestion.value)}>{suggestion.value}</div>;
        });
    }

    render() {
        const suggestions = this.getSuggestions();

        return (<div className="suggestions">
                <Scrollbars style={style.list}
                        onUpdate={this.onScrollUpdate.bind(this)}>
                    {suggestions}
                </Scrollbars>
            </div>);
    }
}

Suggestions.propTypes = {
    onSelectSuggestion: PropTypes.func.isRequired,
    suggestions: PropTypes.array,
    onScrollUpdate: PropTypes.func
};

Suggestions.defaultProps = {
    suggestions: []
};