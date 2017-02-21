import React, { Component, PropTypes } from "react";

const KEY = {
    BACKSPACE: 8,
    TAB: 9,
    ENTER: 13,
    COMMA: 188
};

export default class TagInput extends Component {
    constructor(props) {
        super(props);
        this.state = {
            newTagText: ""
        };
        this.onKeyDown = this.onKeyDown.bind(this);
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick(e) {
        if (!this.node) { return; }
        
        if (this.node.contains(e.target)) {
            return;
        }   

        this.close();
    }

    componentDidMount() {
        this.focusInput();
        document.addEventListener("keypress", this.onKeyDown, false);
        document.addEventListener("click", this.handleClick, false);
    }

    componentWillUnmount() {
        document.removeEventListener("keypress", this.onKeyDown, false);
        this.close();
        document.removeEventListener("click", this.handleClick, false);
        this.node = null;
    }

    onSelectSuggestion(suggestion) {
        if (typeof (this.props.onSelectSuggestion) === "function") {
            this.props.onSelectSuggestion(suggestion);
        }
        this.props.onClose();
    }

    onChange(event) {
        this.setState({ newTagText: event.target.value });
        if (this.props.autoSuggest && typeof(this.props.onAddingNewTagChange) === "function") {
            this.props.onAddingNewTagChange(event.target.value);
        }
    }

    close() {
        this.props.onClose();
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
            case KEY.COMMA:
            case KEY.TAB:
                if (this.state.newTagText) {
                    event.preventDefault();
                    this.props.addTag(this.state.newTagText);
                    this.close();
                }
                break;
            case KEY.BACKSPACE:
                break;
        }
    }

    getSuggestions() {
        if (!this.props.autoSuggest) {
            return null;
        }

        return (<div className="suggestions">
            {this.props.suggestions.map((suggestion, index) => {
                return <div className="suggestion" key={index} onClick={this.onSelectSuggestion.bind(this, suggestion.value)}>{suggestion.value}</div>;
            })}
            </div>);
    }

    focusInput() {
        this.refs.inputField.focus();
    }

    onSuggestionsBlur() {
        const {autoSuggest, suggestions} = this.props;
        if (autoSuggest && suggestions.length == 0) {
            return;
        }

        this.close();
    }
    
    render() {
        const {opts, autoSuggest} = this.props;

        return (
            <div className="input-container" 
                ref={node => this.node = node}>
                <input
                    ref="inputField"
                    type="text"
                    placeholder={__("Add tags")}
                    onKeyDown={this.onKeyDown.bind(this)}
                    value={this.state.newTagText}
                    
                    onChange={this.onChange.bind(this)}
                    {...opts}
                />
                {autoSuggest &&
                    this.getSuggestions()}
            </div>
        );
    }
}

TagInput.propTypes = {
    onAddingNewTagChange: PropTypes.func.isRequired,
    opts: PropTypes.object.isRequired,
    autoSuggest: PropTypes.bool,
    suggestions: PropTypes.array,
    onSelectSuggestion: PropTypes.func.isRequired,
    addTag: PropTypes.func.isRequired,
    onClose: PropTypes.func.isRequired
};

TagInput.defaultProps = {
    suggestions: []
};