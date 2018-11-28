import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import "./style.less";
import { ArrowDownIcon } from "dnn-svg-icons";

class LanguageVerifierRow extends Component {
    componentDIdMount() {
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
    }

    toggle() {
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            this.props.Collapse();
        }
        else {
            this.props.OpenCollapse(this.props.id);
        }
    }    

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        return (
            <div className={"collapsible-component-verifier" + (opened ? " row-opened" : "")}>
                <div className={"collapsible-header-verifier " + !opened} >
                    <div className={"row"}>
                        <div className="verifier-item item-row-name">
                            {props.text}
                        </div>     
                        <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }} onClick={this.toggle.bind(this)} />                    
                    </div>
                </div>
                <Collapse isOpened={opened} style={{ float: "left", width: "100%" }}>{opened && props.children}</Collapse>
            </div>
        );
    }
}

LanguageVerifierRow.propTypes = {
    text: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string
};

LanguageVerifierRow.defaultProps = {
    collapsed: true
};
export default (LanguageVerifierRow);
