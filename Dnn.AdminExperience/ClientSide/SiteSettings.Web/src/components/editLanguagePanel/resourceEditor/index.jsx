import React, { Component } from "react";
import PropTypes from "prop-types";
import debounce from "lodash/debounce";
import FullEditor from "./fullEditor";
import "./style.less";
import { SingleLineInput, MultiLineInput, Modal, SvgIcons } from "@dnnsoftware/dnn-react-common";

class ResourceEditor extends Component {
    constructor(props) {
        super(props);
        this.state = {
            inFullMode: false,
            content: props.value ? props.value : ""
        };
        this.debouncedOnChange = debounce(this.changeContent, 500);
    }

    componentDidUpdate(prevProps) {
        if (this.props.value !== prevProps.value){
            this.setState({content: this.props.value});
        }
    }

    onChange(e) {
        this.setState({content: e.target.value});
        this.debouncedOnChange();
    }

    changeContent() {
        const { props } = this;

        if (props.enabled) {
            props.onChange(this.state.content);
        }
    }

    renderMulti() {
        const { props, state } = this;

        let lines = props.value.length / 30;
        if (props.value.length % 30 !== 0) {
            lines ++;
        }

        let height = lines * 18 + 16;
        if (height > 100) {
            height = 100;
        }

        return (<MultiLineInput 
            className={props.className} 
            value={state.content} 
            enabled={props.enabled}
            style={{height: height + "px"}}
            onChange={this.onChange.bind(this)} />);
    }

    renderSingle() {
        const { props, state } = this;

        return (<SingleLineInput 
            className={props.className} 
            value={state.content} 
            enabled={props.enabled}
            onChange={this.onChange.bind(this)} />);
    }

    onEnterFullMode() {
        this.setState({
            inFullMode: true
        });

        window.dnn.stopEscapeFromClosingPB = true;
    }

    onExitFullMode() {
        this.setState({
            inFullMode: false
        });

        window.dnn.stopEscapeFromClosingPB = false;
    }

    onFullEditorChange(value) {
        const { props } = this;

        props.onChange(value);

        this.onExitFullMode();
    }

    onFullEditorCancel() {
        this.onExitFullMode();
    }
    
    /* eslint-disable react/no-danger */
    render() {
        const { props, state } = this;

        const renderMulti = props.value && props.value.length > 30;

        return (<div className="dnn-language-resource-editor">
            {renderMulti ? this.renderMulti() : this.renderSingle()}
            {props.enabled && 
            <div 
                className="edit-svg" 
                dangerouslySetInnerHTML={{ __html: SvgIcons.EditIcon }}
                onClick={this.onEnterFullMode.bind(this)}>
            </div>
            }
            {props.enabled && 
            <Modal 
                isOpen={state.inFullMode}
                onRequestClose={this.onExitFullMode.bind(this)}
                shouldCloseOnOverlayClick={false}
                modalHeight={390}>
                <FullEditor 
                    value={props.value} 
                    onChange={this.onFullEditorChange.bind(this)}
                    onCancel={this.onFullEditorCancel.bind(this)}
                />
            </Modal>
            }
        </div>);
    }
}

ResourceEditor.propTypes = {
    value: PropTypes.string,
    enabled: PropTypes.bool,
    onChange: PropTypes.func,
    className: PropTypes.string
};

ResourceEditor.defaultProps = {
    enabled: true
};

export default ResourceEditor;
