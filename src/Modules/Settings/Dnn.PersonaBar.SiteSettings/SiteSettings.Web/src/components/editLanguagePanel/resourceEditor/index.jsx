import React, { Component, PropTypes } from "react";
import SingleLineInput from "dnn-single-line-input";
import MultiLineInput from "dnn-multi-line-input";
import FullEditor from "./fullEditor";
import Modal from "dnn-modal";
import { EditIcon } from "dnn-svg-icons";
import resx from "resources";
import utilities from "utils";
import "./style.less";

class ResourceEditor extends Component {
    constructor() {
        super();
        this.state = {
            inFullMode: false
        };
    }

    renderMulti(){
        const { props } = this;

        let lines = props.value.length / 30;
        if(props.value.length % 30 !== 0){
            lines ++;
        }

        let height = lines * 18 + 16;
        if(height > 100){
            height = 100;
        }

        return (<MultiLineInput 
            className={props.className} 
            value={props.value} 
            enabled={props.enabled}
            style={{height: height + "px"}}
            onChange={props.enabled ? props.onChange : null} />);
    }

    renderSingle(){
        const { props } = this;

        return (<SingleLineInput 
            className={props.className} 
            value={props.value} 
            enabled={props.enabled}
            onChange={props.enabled ? props.onChange : null} />);
    }

    onEnterFullMode(){
        this.setState({
            inFullMode: true
        });

        window.dnn.stopEscapeFromClosingPB = true;
    }

    onExitFullMode(){
        this.setState({
            inFullMode: false
        });

        setTimeout(() => {
            window.dnn.stopEscapeFromClosingPB = false;
        }, 500);
    }

    onFullEditorChange(value){
        const { props } = this;

        props.onChange(value);

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
                dangerouslySetInnerHTML={{ __html: EditIcon }}
                onClick={this.onEnterFullMode.bind(this)}>
            </div>
            }
            {props.enabled && 
            <Modal 
                isOpen={state.inFullMode}
                onRequestClose={this.onExitFullMode.bind(this)}
                shouldCloseOnOverlayClick={false}
                modalHeight={390}
                >
                <FullEditor 
                    value={props.value} 
                    onChange={this.onFullEditorChange.bind(this)} />
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
