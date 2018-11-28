import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import "./style.less";
import { EditIcon, TrashIcon, DragRowIcon } from "dnn-svg-icons";

class ListEntryRow extends Component {
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
    render() {
        const { props } = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        return (
            <div className={"collapsible-component-entry" + (opened ? " row-opened" : "")}>
                <div className={"collapsible-entry " + !opened} >
                    <div className={"row"}>
                        <div title={props.name} className="list-item item-row-text">
                            {props.text}
                        </div>
                        <div className="list-item item-row-value">
                            {props.value}
                        </div>
                        <div className="list-item item-row-actionButtons">
                            {props.enableSortOrder &&
                                <div className={opened ? "order-icon-hidden" : "order-icon"}
                                    dangerouslySetInnerHTML={{ __html: DragRowIcon }} >
                                </div>
                            }
                            <div className={opened ? "delete-icon-hidden" : "delete-icon"}
                                dangerouslySetInnerHTML={{ __html: TrashIcon }}
                                onClick={props.onDelete.bind(this, props.entryId)}>
                            </div>
                            <div className={opened ? "edit-icon-active" : "edit-icon"}
                                dangerouslySetInnerHTML={{ __html: EditIcon }}
                                onClick={this.toggle.bind(this)}>
                            </div>
                        </div>
                    </div>
                </div>
                <Collapse fixedHeight={props.enableSortOrder === undefined || props.enableSortOrder === null ? 270 : 220}
                    isOpened={opened}
                    style={{ float: "left", width: "100%", overflow: "inherit" }}>
                    {opened && props.children}
                </Collapse>
            </div>
        );
    }
}

ListEntryRow.propTypes = {
    entryId: PropTypes.number,
    value: PropTypes.string,
    text: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    enableSortOrder: PropTypes.bool
};

ListEntryRow.defaultProps = {
    collapsed: true
};

export default (ListEntryRow);
