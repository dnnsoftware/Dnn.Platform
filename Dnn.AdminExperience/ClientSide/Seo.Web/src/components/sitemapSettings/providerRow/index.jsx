import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible as Collapse, SvgIcons } from "@dnnsoftware/dnn-react-common";
import resx from "../../../resources";
import "./style.less";

class ProviderRow extends Component {
    constructor() {
        super();
    }

    UNSAFE_componentWillMount() {
        let opened = (this.props.openId !== "" && this.props.name === this.props.openId);
        this.setState({
            opened
        });
    }

    toggle() {
        if ((this.props.openId !== "" && this.props.name === this.props.openId)) {
            //this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.name);
        }
    }

    /* eslint-disable react/no-danger */
    getEnabledDisplay() {
        const {props} = this;
        if (props.enabled) {
            return (
                <div className="item-row-enabled-display">
                    <div className="enabled-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.CheckMarkIcon }} />
                </div>
            );
        }
        else {
            return (
                <div>&nbsp;</div>
            );
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.name === this.props.openId);
        if (props.visible) {
            return (
                <div className={"collapsible-component-providers" + (opened ? " row-opened" : "")}>
                    <div className={"collapsible-providers " + !opened} >
                        <div className={"row"}>
                            <div className="provider-item item-row-name">
                                {props.name}
                            </div>
                            <div className="provider-item item-row-enabled">
                                {this.getEnabledDisplay()}
                            </div>
                            <div className="provider-item item-row-priority">
                                {props.overridePriority ? props.priority : resx.get("None")}
                            </div>
                            <div className="provider-item item-row-editButton">
                                <div className={opened ? "edit-icon-active" : "edit-icon"} dangerouslySetInnerHTML={{ __html: SvgIcons.EditIcon }} onClick={this.toggle.bind(this)} />
                            </div>
                        </div>
                    </div>
                    <Collapse accordion={true} isOpened={opened} style={{ float: "left", overflow: "inherit" }} fixedHeight={160}>{opened && props.children}</Collapse>
                </div>
            );
        }
        else {
            return <div />;
        }
    }
}

ProviderRow.propTypes = {
    name: PropTypes.string,
    enabled: PropTypes.bool,
    priority: PropTypes.number,
    overridePriority: PropTypes.bool,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    visible: PropTypes.bool
};

ProviderRow.defaultProps = {
    collapsed: true,
    visible: true
};

export default (ProviderRow);
