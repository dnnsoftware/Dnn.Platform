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

    renderActions(){
        const {props} = this;

        let components = [];
        let actionTemplate = props.linkInputActionText;

        let tokenRegex = /\{(.+?)\|(.+?)\}/;
        while(tokenRegex.test(actionTemplate)){
            let match = tokenRegex.exec(actionTemplate);

            components.push(actionTemplate.substr(0, match.index));

            let action = ((type) => {
                switch(type.toLowerCase()){
                    case "save":
                        return this.onSave.bind(this);
                    case "cancel":
                        return this.props.onCancel;
                    default:
                        return null;
                }
            })(match[1]);

            components.push(<strong onClick={action}>{match[2]}</strong>);

            actionTemplate = actionTemplate.substr(match.index + match[0].length);
        }

        if(actionTemplate){
            components.push(actionTemplate);
        }

        return components;
    }

    render() {
        return <div className="file-upload-container">
            <h4>{this.props.linkInputTitleText}</h4>
            <div className="textarea-container">
                <textarea 
                    value={this.state.url} 
                    onChange={this.onChange.bind(this) } 
                    placeholder={this.props.linkInputPlaceholderText} 
                    aria-label="Link" />
                <span>{this.renderActions()}</span>
            </div>
        </div>;
    }
}

LinkInput.propTypes = {
    linkPath: PropTypes.string.isRequired,
    onSave: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,

    linkInputTitleText: PropTypes.string,
    linkInputPlaceholderText: PropTypes.string,
    linkInputActionText: PropTypes.string
};