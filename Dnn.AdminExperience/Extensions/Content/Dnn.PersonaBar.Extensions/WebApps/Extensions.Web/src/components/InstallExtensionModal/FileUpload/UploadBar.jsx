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
        this.delta = 1.00;
        this.setTimeout = null;
    }

    componentDidMount() {
        setTimeout(this.increase.bind(this), 100);
        this._isMounted = true;
    }

    componentWillUnmount() {
        this._isMounted = false;
    }

    UNSAFE_componentWillReceiveProps(props) {
        if (props.uploadComplete) {
            clearTimeout(this.setTimeout);
            if (this._isMounted) {
                this.setState({ percent: 100 }, () => {
                });
            }
        }
    }

    increase() {
        let {percent} = this.state;
        percent++;
        this.timeout *= this.delta;
        this.delta *= 1.00;
        if (percent <= 100) {
            if (this._isMounted) {
                this.setState({ percent });
            }
        }
        if (percent < 95) {
            this.setTimeout = setTimeout(this.increase.bind(this), this.timeout);
        }
    }

    render() {
        const { props } = this;
        /* eslint-disable react/no-danger */
        let percent = props.errorText ? 0 : this.state.percent;
        let text = props.uploadComplete ? props.uploadCompleteText : props.uploadingText;
        text = props.errorText ? props.errorText : text;
        let svg = props.uploadComplete ? checkmark : upload;
        svg = props.errorText ? errorIcon : svg;
        const className = "file-upload-container dnn-upload-bar" + (props.uploadComplete ? " complete" : "") + (props.errorText ? " upload-error" : "");

        return <div className={className}>
            <div className="upload-bar-container">
                <div className="upload-file-name">{this.props.fileName || "myImage.jpg"}</div>
                <div className="upload-icon" dangerouslySetInnerHTML={{ __html: svg }} />
                <h4>{text}</h4>
                {props.errorInPackage &&
                    <p className="view-log-or-try-again">
                        <span onClick={props.onViewLog.bind(this)}>{props.viewLogText} </span>
                        or
                        <span onClick={props.onTryAgain.bind(this)}>{props.tryAgainText}</span>
                    </p>}
                <div className="upload-percent">{percent + "%"}</div>
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{ width: percent + "%" }} />
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
    errorInPackage: PropTypes.bool
};

UploadBar.defaultProps = {
    uploadCompleteText: "Upload Complete",
    uploadingText: "Uploading..."
};