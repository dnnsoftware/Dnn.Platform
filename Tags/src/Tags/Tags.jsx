import React, { Component, PropTypes } from "react";
import Tag from "../Tag/Tag";
import "./style.less";

const KEY = {
    BACKSPACE: 8,
    TAB: 9,
    ENTER: 13,
    COMMA: 188
};

/**
 *  
 */
export default class Tags extends Component {
    constructor() {
        super();
        this.state = {
            newTagText: "",
            inputWidth: 0,
            isInputVisible: true,
            focus: false
        };
    }

    componentDidMount() {
        this.state = {
            tags: this.props.tags,
            newTagText: "",
            inputWidth: this.refs.tagsField.getBoundingClientRect().width
        };
        setTimeout(this.resizeInputField.bind(this), 0);
    }

    hasClass(className, element) {
        return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
    }

    resizeInputField() {
        const fieldWidth = this.refs.tagsField.getBoundingClientRect().width;
        let tagsWidth = 0;
        [].forEach.call(this.refs.tagsField.childNodes[0].childNodes, (tag) => {
            if (this.hasClass("dnn-uicommon-tag-input", tag)) {
                tagsWidth = tagsWidth + tag.getBoundingClientRect().width + 8;
                if ((fieldWidth - tagsWidth) < 60) {
                    tagsWidth = 0;
                }
            }
        });
        this.setState({ inputWidth: fieldWidth - tagsWidth - 25, isInputVisible: true });
    }

    addTag() {
        const newTagText = this.state.newTagText;
        if (!newTagText) {
            return;
        }
        if (this.props.tags.find(t=> t === newTagText)) {
            this.setState({ newTagText: "" });
            return;
        }

        this.setState({ isInputVisible: false });
        const tags = this.props.tags.slice();
        tags.push(newTagText.trim());         
        this.updateTags(tags);
        this.setState({ newTagText: "" });
        setTimeout(() => { this.refs.inputField.focus(); }, 0);
    }

    removeTagByName(tag) {
        const tags = this.props.tags.filter((_tag) => { return _tag !== tag; }).slice();
        this.updateTags(tags);
    }

    removeLastTag() {
        if (this.state.newTagText) {
            return;
        }
        const tags = this.props.tags.slice();
        tags.pop();
        this.updateTags(tags);
    }

    updateTags(tags) {
        this.props.onUpdateTags(tags);
        setTimeout(this.resizeInputField.bind(this), 0);
    }

    onKeyDown(event) {
        
        switch (event.keyCode) {
            case KEY.ENTER:
                return this.addTag();
            case KEY.COMMA:
                return this.addTag();
            case KEY.TAB:
                event.preventDefault();
                return this.addTag();
            case KEY.BACKSPACE:
                return this.removeLastTag();
        }
    }

    onChange(event) {
        event;
        this.setState({ newTagText: event.target.value });
    }

    render() {
        const Tags = this.props.tags.map((tag, index) => {
            return <Tag tag={tag} key={index} onRemove={this.removeTagByName.bind(this) }/>;
        });
        const inputStyle = {
            width: this.state.inputWidth,
            display: (this.state.isInputVisible === false ? "none" : "block")
        };
        return (
            <div className={"dnn-uicommon-tags-field-input" + (this.state.focus ? " focus" : "")} 
                ref="tagsField" style={this.props.style}>
                <div type="text">
                    {Tags}
                    <input
                        ref="inputField"
                        type="text"
                        placeholder="Add Tag"
                        style={inputStyle}
                        onKeyDown={this.onKeyDown.bind(this) }
                        value={this.state.newTagText}
                        onChange={this.onChange.bind(this) }
                        onFocus={() => this.setState({focus: true})}
                        onBlur={() => { this.addTag(); this.setState({focus: false});}} />
                </div>                
            </div>
        );
    }
}

Tags.propTypes = {
    tags: PropTypes.array.isRequired,
    onUpdateTags: PropTypes.func.isRequired,
    style: PropTypes.object
};