import React, { Component } from "react";
import PropTypes from "prop-types";

const upload = require("!raw-loader!./img/upload.svg").default;
const checkmark = require("!raw-loader!./img/checkmark.svg").default;
const errorIcon = require("!raw-loader!./img/x.svg").default;

export default class UploadBar extends Component {
    constructor() {
        super();
        this.state = {
            percent: 0
        };
        this.timeout = 20;
        this.delta = 1.01;
        this.setTimeout = null;
    }

    componentDidMount() {
        setTimeout(this.increase.bind(this), 100);
    }
    
    componentDidUpdate(prevProps) {
        const { props } = this;
        if (props.uploadComplete && props.uploadComplete !== prevProps.uploadComplete) {
            clearTimeout(this.setTimeout);
            this.setState({percent: 100});
        }
    }

    increase() {
        let {percent} = this.state;
        percent++;
        this.timeout *= this.delta;
        this.delta *= 1.001;
        this.setState({percent});
        if (percent < 99) {
            this.setTimeout = setTimeout(this.increase.bind(this), this.timeout);
        }
    }

    render() {
        /* eslint-disable react/no-danger */
        let percent = this.props.errorText ? 0 : this.state.percent;
        let text = this.props.uploadComplete ? this.props.uploadCompleteText : this.props.uploadingText;
        text = this.props.errorText ? this.props.errorText : text;
        let svg = this.props.uploadComplete ? checkmark : upload;
        svg = this.props.errorText ? errorIcon : svg;
        const className = "file-upload-container dnn-upload-bar" + (!this.props.errorText && this.props.uploadComplete ? " complete" : "") + (this.props.errorText ? " upload-error" : "");

        return <div className={className}>
            <div className="upload-bar-container">
                <div className="upload-file-name">{this.props.fileName || this.props.uploadDefaultText}</div>
                <div className="upload-icon" dangerouslySetInnerHTML={{ __html: svg }} />
                <h4>{text}</h4>
                <div className="upload-percent">{percent + "%"}</div>
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{width: percent + "%"}}/>
                </div>
            </div>
        </div>;
    }
}

UploadBar.propTypes = {
    errorText: PropTypes.string.isRequired, 
    fileName: PropTypes.string.isRequired,
    uploadComplete: PropTypes.bool.isRequired,

    uploadCompleteText: PropTypes.string,
    uploadingText: PropTypes.string,
    uploadDefaultText: PropTypes.string
};