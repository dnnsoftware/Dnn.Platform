import React, { Component, PropTypes } from "react";

const KEY = {
    BACKSPACE: 8,
    TAB: 9,
    ENTER: 13,
    COMMA: 188,
    DOWN_ARROW: 40,
    UP_ARROW: 38
};

export default class TagInput extends Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick(e) {
        if (!this.props.container || this.props.container.contains(e.target)) {
            return;
        }   

        if (this.props.newTagText) {
            this.props.addTag(this.props.newTagText);
        }
        
        this.close();
    }

    componentDidMount() {
        this.focusInput();
        document.addEventListener("click", this.handleClick, true);
    }

    componentWillUnmount() {
        this.close();
        document.removeEventListener("click", this.handleClick, true);
    }

    addTag(tag) {
        this.props.addTag(tag);
        
        const inputField = this.refs.inputField;
        setTimeout(() => { inputField.focus(); }, 0);
    }

    onChange(event) {
        this.props.onAddingNewTagChange(event.target.value);        
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
                event.preventDefault();
                this.addTag(this.props.newTagText);
                break;
            case KEY.BACKSPACE:
                this.removeLastTag();
                break;
            case KEY.DOWN_ARROW:
                this.props.onArrowDown();
                break;
            case KEY.UP_ARROW:
                this.props.onArrowUp();
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
            <div>
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
    onFocus: PropTypes.func,
    container: PropTypes.object,
    onArrowUp: PropTypes.func.isRequired,
    onArrowDown: PropTypes.func.isRequired
};
