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

    addTag(tag) {
        if (typeof (this.props.addTag) === "function") {
            this.props.addTag(tag);    
        }
        
        const inputField = this.refs.inputField;
        setTimeout(() => { inputField.focus(); }, 0);
    }

    onChange(event) {
        if (typeof(this.props.onAddingNewTagChange) === "function") {
            this.props.onAddingNewTagChange(event.target.value);
        }
    }

    close() {
        this.props.onClose();
    }

    removeLastTag() {
        if (this.props.newTagText) {
            return;
        }
        this.props.removeLastTag();
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
            case KEY.COMMA:
            case KEY.TAB:
                if (this.props.newTagText) {
                    event.preventDefault();
                    this.addTag(this.props.newTagText);
                }
                break;
            case KEY.BACKSPACE:
                this.removeLastTag();
                break;
        }
    }

    focusInput() {
        this.refs.inputField.focus();
        if (typeof(this.props.onFocus) === "function") {
            this.props.onFocus(this.props.newTagText);
        }
    }

    render() {
        const {opts} = this.props;

        return (
            <div
                ref={node => this.node = node}>
                <div className="input-container">
                    <input
                        ref="inputField"
                        type="text"
                        placeholder={this.props.addTagsPlaceholder}
                        onKeyDown={this.onKeyDown.bind(this)}
                        value={this.props.newTagText}
                        aria-label="Tag"
                        onChange={this.onChange.bind(this)}
                        {...opts}
                    />
                </div>
            </div>
        );
    }
}

TagInput.propTypes = {
    newTagText: PropTypes.string.isRequired,
    onAddingNewTagChange: PropTypes.func.isRequired,
    opts: PropTypes.object.isRequired,
    addTag: PropTypes.func.isRequired,
    onClose: PropTypes.func.isRequired,
    removeLastTag: PropTypes.func.isRequired,
    addTagsPlaceholder: PropTypes.string.isRequired,
    onFocus: PropTypes.func
};
