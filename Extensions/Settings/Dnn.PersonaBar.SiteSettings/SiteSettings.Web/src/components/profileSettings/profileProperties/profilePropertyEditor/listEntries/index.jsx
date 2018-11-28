import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "actions";
import ListEntryRow from "./listEntryRow";
import ListEntryEditor from "./listEntryEditor";
import Collapse from "dnn-collapsible";
import "./style.less";
import { AddIcon } from "dnn-svg-icons";
import Sortable from "dnn-sortable";
import util from "utils";
import resx from "resources";

let tableFields = [];

class ListEntriesPanel extends Component {
    constructor() {
        super();
        this.state = {
            openId: ""
        };
    }

    componentDidMount() {
        const { props } = this;
        if (tableFields.length === 0) {
            tableFields.push({ "name": resx.get("ListEntryText"), "id": "Text" });
            tableFields.push({ "name": resx.get("ListEntryValue"), "id": "Value" });
        }
        props.dispatch(SiteBehaviorActions.getListInfo(props.listName, props.portalId));
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "list-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="list-header-row">{tableHeaders}</div>;
    }

    uncollapse(id) {
        this.setState({
            openId: id
        });
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        }
    }

    onUpdateEntry(entryDetail) {
        const { props } = this;

        const entry = Object.assign({}, entryDetail);
        props.dispatch(SiteBehaviorActions.updateListEntry(entry, () => {
            util.utilities.notify(entry.EntryId ? resx.get("ListEntryUpdateSuccess") : resx.get("ListEntryCreateSuccess"));
            this.collapse();
            props.dispatch(SiteBehaviorActions.getListInfo(props.listName, props.portalId));
        }, (error) => {
            const errorMessage = JSON.parse(error.responseText);
            util.utilities.notifyError(errorMessage.Message);
        }));
    }

    onDeleteEntry(entryId) {
        const { props } = this;
        util.utilities.confirm(resx.get("ListEntryDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteBehaviorActions.deleteListEntry(entryId, props.portalId, () => {
                util.utilities.notify(resx.get("ListEntryDeleteSuccess"));
                this.collapse();
                props.dispatch(SiteBehaviorActions.getListInfo(props.listName, props.portalId));
            }, () => {
                util.utilities.notify(resx.get("ListEntryDeleteError"));
            }));
        });
    }

    /* eslint-disable react/no-danger */
    renderedListEntries() {
        let i = 0;
        return this.props.entries.map((item, index) => {
            let id = "row-" + i++;
            return (
                <ListEntryRow
                    entryId={item.EntryID}
                    text={item.Text}
                    value={item.Value}
                    index={index}
                    key={"listItem-" + index}
                    closeOnClick={true}
                    openId={this.state.openId}
                    OpenCollapse={this.toggle.bind(this)}
                    Collapse={this.collapse.bind(this)}
                    onDelete={this.onDeleteEntry.bind(this, item.EntryID)}
                    enableSortOrder={this.props.enableSortOrder}
                    id={id}>
                    <ListEntryEditor
                        portalId={this.props.portalId}
                        entryId={item.EntryID}
                        listName={this.props.listName}
                        text={item.Text}
                        value={item.Value}
                        enableSortOrder={this.props.enableSortOrder}
                        Collapse={this.collapse.bind(this)}
                        onUpdate={this.onUpdateEntry.bind(this)}
                        id={id}
                        openId={this.state.openId} />
                </ListEntryRow>
            );
        });
    }

    onSort(items) {
        const { props } = this;
        props.dispatch(SiteBehaviorActions.updateListEntryOrders(
            {
                PortalId: props.portalId,
                Entries: items
            }, items, () => {
                this.collapse();
            }));
    }

    render() {
        let opened = (this.state.openId === "add");
        return (
            <div>
                <div className="list-items">
                    <div className="AddItemRow">
                        <div className="sectionTitle">
                            {resx.get("ListEntries")}
                        </div>
                        <div className={opened ? "AddItemBox-active" : "AddItemBox"}
                            onClick={this.toggle.bind(this, opened ? "" : "add")}>
                            <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                            </div> {resx.get("cmdAddEntry")}
                        </div>
                    </div>
                    <div className="list-items-grid">
                        {this.renderHeader()}
                        <Collapse isOpened={opened}
                            style={{ float: "left", width: "100%" }}>
                            <ListEntryRow
                                text={"-"}
                                value={"-"}
                                index={"add"}
                                key={"listItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                onDelete={this.onDeleteEntry.bind(this)}
                                enableSortOrder={this.props.enableSortOrder}
                                id={"add"}>
                                <ListEntryEditor
                                    portalId={this.props.portalId}
                                    listName={this.props.listName}
                                    text={""}
                                    value={""}
                                    enableSortOrder={this.props.enableSortOrder}
                                    Collapse={this.collapse.bind(this)}
                                    onUpdate={this.onUpdateEntry.bind(this)}
                                    id={"add"}
                                    openId={this.state.openId} />
                            </ListEntryRow>
                        </Collapse>
                        {this.props.entries &&
                            this.props.entries.length > 0 &&
                            (this.state.openId || !this.props.enableSortOrder) &&
                            this.renderedListEntries()
                        }
                        {this.props.entries &&
                            this.props.entries.length > 0 &&
                            !this.state.openId &&
                            this.props.enableSortOrder &&
                            <Sortable
                                onSort={this.onSort.bind(this)}
                                items={this.props.entries}
                                sortOnDrag={true}>
                                {this.renderedListEntries()}
                            </Sortable>
                        }
                        {this.props.entries && this.props.entries.length === 0 &&
                            <div className="no-data">{resx.get("NoData")}</div>
                        }
                    </div>
                </div>
            </div >
        );
    }
}

ListEntriesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    portalId: PropTypes.number,
    listName: PropTypes.string,
    entries: PropTypes.array,
    enableSortOrder: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        entries: state.siteBehavior.entries,
        enableSortOrder: state.siteBehavior.enableSortOrder
    };
}

export default connect(mapStateToProps)(ListEntriesPanel);