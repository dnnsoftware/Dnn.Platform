import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import Checkbox from "dnn-checkbox";
import "./style.less";
import { EditIcon } from "dnn-svg-icons";

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
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.name);
        }
    }

    updateStatus(event) {
        const {props} = this;
        this.props.onUpdateStatus(props.providerId, event);
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.name === this.props.openId);
        if (props.visible) {
            return (
                <div className={"collapsible-component-extention-url-providers" + (opened ? " row-opened" : "")}>
                    <div className={"collapsible-providers " + !opened} >
                        <div className={"row"}>
                            <div className="provider-item item-row-name">
                                {props.name}
                            </div>
                            <div className="provider-item item-row-enabled">
                                <Checkbox value={props.enabled} onChange={this.updateStatus.bind(this) } />
                            </div>
                            <div className="provider-item item-row-editButton">
                                <div className={opened ? "edit-icon-active" : "edit-icon"} dangerouslySetInnerHTML={{ __html: EditIcon }} onClick={this.toggle.bind(this)} />
                            </div>
                        </div>
                    </div>
                    <Collapse accordion={true} isOpened={opened} style={{ float: "left", overflow: "inherit", width: "100%" }} fixedHeight={160}>{opened && props.children}</Collapse>
                </div>
            );
        }
        else {
            return <div />;
        }
    }
}

ProviderRow.propTypes = {
    providerId: PropTypes.number,
    name: PropTypes.string,
    enabled: PropTypes.bool,    
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    visible: PropTypes.bool,
    onUpdateStatus: PropTypes.func
};

ProviderRow.defaultProps = {
    collapsed: true,
    visible: true
};

export default (ProviderRow);
