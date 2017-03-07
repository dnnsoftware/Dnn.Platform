import React, {Component, PropTypes} from "react";

const KEY = {
    ENTER: 13,
    ESCAPE: 27
};

export default class LinkInput extends Component {
    constructor(props) {
        super(props);
        const url = this.props.linkPath; 
        this.state = {url};
        this.onKeyDown = this.onKeyDown.bind(this);
    }

    componentDidMount() {
        document.addEventListener("keyup", this.onKeyDown, false);
    }

    componentWillUnmount() {
        document.removeEventListener("keyup", this.onKeyDown, false);
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
                return this.onSave();
            case KEY.ESCAPE:
                return this.props.onCancel();
        }
    }

    onSave() {
        if (!this.state.url) {
            return this.props.onCancel();
        }
        this.props.onSave(this.state.url, true);
    }

    onChange(e) {
        this.setState({ url: e.target.value });
    }

    render() {
        return <div className="file-upload-container">
            <h4>{"URL Link"}</h4>
            <div className="textarea-container">
                <textarea value={this.state.url} onChange={this.onChange.bind(this) } placeholder="http://example.com/imagename.jpg" aria-label="Link" />
                <span>Press <strong onClick={this.onSave.bind(this)}>[ENTER]</strong> to save, or <strong onClick={this.props.onCancel}>[ESC]</strong> to cancel</span>
            </div>
        </div>;
    }
}

LinkInput.propTypes = {
    linkPath: PropTypes.string.isRequired,
    onSave: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired
};