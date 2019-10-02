import React, { Component } from "react";
import PropTypes from "prop-types";
import { Button, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import resx from "resources";
import "./fullEditor.less";

class FullEditor extends Component {
    constructor() {
        super();
        this.state = {
        };

        this.editorToolbar = [
            ["undo", "redo"],
            ["bold", "italic", "underline", "strikeThrough"],
            ["justifyLeft", "justifyCenter", "justifyRight", "justifyFull"],
            ["insertUnorderedList", "insertOrderedList"]
        ];
    }

    componentDidMount() {
        this.editorControl.focus();
    }

    execCommand(button, e) {
        this.editorControl.focus();
        document.execCommand(button, false, null);
        
        e.preventDefault();
    }

    onSave() {
        const { props } = this;

        if (typeof props.onChange === "function") {
            let content = this.editorControl.innerHTML;
            props.onChange(content);
        }
    }

    onCancel() {
        const { props } = this;

        props.onCancel();
    }

    /* eslint-disable react/no-danger */
    renderToolbar() {
        return (
            <div className='fulleditor-controls'>
                {
                    this.editorToolbar.map((group, i) => {
                        return <div className='btn-group' key={i}>
                            {
                                group.map((button, i) => {
                                    return (
                                        <a 
                                            className='btn' 
                                            data-role={button} 
                                            href='#'
                                            onClick={this.execCommand.bind(this, button)}
                                            dangerouslySetInnerHTML={{ __html: require("./icons/" + button + ".svg") }}
                                            key={i}>
                                        </a>
                                    );
                                })
                            }
                        </div>;
                    })
                }
            </div>
        );
    }

    /* eslint-disable react/no-danger */
    renderEditor() {
        const { props } = this;
        return (
            <div className='fulleditor-editor' 
                contentEditable
                ref={(e) => { this.editorControl = e; }}
                dangerouslySetInnerHTML={{ __html: props.value }}>
            </div>
        );
    }

    renderButtons() {
        return [
            <Button type="secondary" onClick={this.onCancel.bind(this)} key="first">
                <TextOverflowWrapper text={resx.get("Cancel") } maxWidth={100} />
            </Button>,
            <Button type="primary" onClick={this.onSave.bind(this)} key="second">
                <TextOverflowWrapper text={resx.get("Save") } maxWidth={100} />
            </Button>
        ];
    }

    render() {
        return (
            <div className="dnn-language-resource-full-editor">
                {this.renderToolbar()}
                {this.renderEditor()}
                {this.renderButtons()}
            </div>
        );
    }
}

FullEditor.propTypes = {
    value: PropTypes.string,
    onChange: PropTypes.func,
    className: PropTypes.string
};

FullEditor.defaultProps = {
};

export default FullEditor;
