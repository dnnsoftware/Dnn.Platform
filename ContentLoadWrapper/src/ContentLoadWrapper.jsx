import React, { Component, PropTypes } from "react";
import { ErrorStateIcon } from "dnn-svg-icons";
import "./style.less";

export default class ContentLoadWrapper extends Component {
    constructor() {
        super();
        this.state = {
            percent: 0
        };
        this.timeout = 50;
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

    componentWillReceiveProps(props) {
        if (props.loadComplete) {
            clearTimeout(this.setTimeout);
            if (this._isMounted) {
                this.setState({ percent: 100 }, () => {
                });
            }
            setTimeout(() => {
                if (typeof props.onCompleteCallback === "function") {
                    props.onCompleteCallback();
                }
            }, 300);
        }
        if (props.loadError) {
            clearTimeout(this.setTimeout);
            if (this._isMounted) {
                this.setState({ percent: 100 }, () => {
                });
            }
            setTimeout(() => {
                if (typeof props.onErrorCallback === "function") {
                    props.onErrorCallback();
                }
            }, 300);
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
    onTryAgain() {
        this.setState({
            percent: 0
        }, () => {
            setTimeout(this.increase.bind(this), 100);
        });
        if (typeof this.props.onTryAgain === "function") {
            this.props.onTryAgain();
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        let {percent} = this.state;
        const className = "dnn-content-load-wrapper" + (this.props.loadComplete ? " complete" : "") + (this.props.loadError ? " upload-error" : "");
        if (typeof this.props.loadComplete === "undefined" || this.props.loadComplete) {
            return this.props.children;
        }

        return <div className={className}>
            {this.props.svgSkeleton}
            {this.props.loadError &&
                <div className="try-load-again">
                    <div>
                        <div className="upload-icon" dangerouslySetInnerHTML={{ __html: ErrorStateIcon }} />
                        <p>{this.props.failedToLoadText}</p>
                        <span onClick={this.onTryAgain.bind(this)}>[{this.props.tryAgainText}]</span>
                    </div>
                </div>}
            <div className="upload-bar-container">
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{ width: percent + "%" }} />
                </div>
            </div>
        </div>;
    }
}

ContentLoadWrapper.propTypes = {
    loadComplete: PropTypes.bool.isRequired,
    failedToLoadText: PropTypes.string,
    children: PropTypes.node,
    svgSkeleton: PropTypes.node,
    tryAgainText: PropTypes.string,
    loadError: PropTypes.bool,
    onTryAgain: PropTypes.func
};

ContentLoadWrapper.defaultProps = {
    failedToLoadText: "Failed To Load",
    tryAgainText: "Retry"
};