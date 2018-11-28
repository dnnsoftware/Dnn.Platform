import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import "./style.less";
import { CheckMarkIcon, EditIcon, TrashIcon } from "dnn-svg-icons";

class SiteAliasRow extends Component {
    componentDidMount() {
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
    }

    toggle() {
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            //this.props.Collapse();
        }
        else {
            this.props.OpenCollapse(this.props.id);
        }
    }

    /* eslint-disable react/no-danger */
    getBooleanDisplay(prop) {
        if (this.props.id !== "add") {
            if (prop) {
                return <div className="checkMarkIcon" dangerouslySetInnerHTML={{ __html: CheckMarkIcon }}></div>;
            }
            else return <span>&nbsp; </span>;
        }
        else return <span>-</span>;
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        return (
            <div className={"collapsible-component-alias" + (opened ? " row-opened" : "")}>
                <div className={"collapsible-alias " + !opened} >
                    <div className={"row"}>
                        <div title={props.name} className="alias-item item-row-alias">
                            {props.alias}</div>
                        <div className="alias-item item-row-browser">
                            {props.browser}</div>
                        <div className="alias-item item-row-theme">
                            {props.skin}&nbsp;</div>
                        {props.showLanguageColumn &&
                            <div className="alias-item item-row-language">
                                {props.language}&nbsp;</div>
                        }
                        <div className="alias-item item-row-primary">
                            {this.getBooleanDisplay(props.isPrimary)}</div>
                        <div className="alias-item item-row-actionButtons">
                            {props.deletable &&
                                <div className={opened ? "delete-icon-hidden" : "delete-icon"} dangerouslySetInnerHTML={{ __html: TrashIcon }} onClick={props.onDelete.bind(this)}></div>
                            }
                            {props.editable &&
                                <div className={opened ? "edit-icon-active" : "edit-icon"} dangerouslySetInnerHTML={{ __html: EditIcon }} onClick={this.toggle.bind(this)}></div>
                            }
                        </div>
                    </div>
                </div>
                <Collapse fixedHeight={320} keepContent={true} isOpened={opened} style={{ float: "left", width: "100%", overflow: "inherit" }}>{opened && props.children}</Collapse>
            </div>
        );
    }
}

SiteAliasRow.propTypes = {
    aliasId: PropTypes.number,
    alias: PropTypes.string,
    browser: PropTypes.string,
    skin: PropTypes.string,
    language: PropTypes.string,
    isPrimary: PropTypes.bool,
    deletable: PropTypes.bool,
    editable: PropTypes.bool,
    showLanguageColumn: PropTypes.bool,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string
};

SiteAliasRow.defaultProps = {
    collapsed: true
};
export default (SiteAliasRow);
