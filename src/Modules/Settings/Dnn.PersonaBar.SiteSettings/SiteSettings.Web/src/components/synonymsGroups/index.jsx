import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    search as SearchActions
} from "../../actions";
import SynonymsGroupRow from "./synonymsGroupRow";
import SynonymsGroupEditor from "./synonymsGroupEditor";
import Collapse from "react-collapse";
import "./style.less";
import { AddIcon } from "dnn-svg-icons";
import util from "../../utils";
import resx from "../../resources";

let tableFields = [];

class SynonymsGroupsPanel extends Component {
    constructor() {
        super();
        this.state = {
            synonymsGroups: [],
            openId: ""
        };
    }

    componentWillMount() {
        const {props} = this;

        if (tableFields.length === 0) {
            tableFields.push({ "name": resx.get("SynonymsGroup.Header"), "id": "Synonyms" });
        }

        if (props.synonymsGroups) {
            this.setState({
                synonymsGroups: props.synonymsGroups
            });
            return;
        }
        props.dispatch(SearchActions.getSynonymsGroups(props.portalId, props.cultureCode ? props.cultureCode : '', (data) => {
            this.setState({
                synonymsGroups: Object.assign({}, data.SynonymsGroups)
            });
        }));
    }

    componentWillReceiveProps(props) {
        let {state} = this;        

        if (props.synonymsGroups) {
            this.setState({
                synonymsGroups: props.synonymsGroups
            });
            return;
        }
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "synonyms-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="header-row">{tableHeaders}</div>;
    }

    uncollapse(id) {
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, this.timeout);
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

    onUpdateSynonymsGroup(group) {
        const {props, state} = this;
        let groups = []; 
        if (group.SynonymsGroupId) {
            groups = props.synonymsGroups.map((item, index) => {
                if(item.SynonymsGroupId === group.SynonymsGroupId) {
                    return group;
                }
                else{
                    return item;
                }
            });

            props.dispatch(SearchActions.updateSynonymsGroup(group, groups, (data) => {
                util.utilities.notify(resx.get("SynonymsGroupUpdateSuccess"));
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(SearchActions.addSynonymsGroup(group, props.synonymsGroups, (data) => {
                util.utilities.notify(resx.get("SynonymsGroupCreateSuccess"));
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onDeleteSynonymsGroup(group) {
        const {props, state} = this;
        util.utilities.confirm(resx.get("SynonymsGroupDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            const itemList = props.synonymsGroups.filter((item) => item.SynonymsGroupId !== group.SynonymsGroupId);
            props.dispatch(SearchActions.deleteSynonymsGroup(group, itemList, () => {
                util.utilities.notify(resx.get("SynonymsGroupDeleteSuccess"));
                this.collapse();
            }, (error) => {
                util.utilities.notify(resx.get("SynonymsGroupDeleteError"));
            }));
        });
    }

    /* eslint-disable react/no-danger */
    renderedSynonymsGroups() {
        let i = 0;
        if (this.props.synonymsGroups) {
            return this.props.synonymsGroups.map((item, index) => {
                let id = "row-" + i++;
                return (
                    <SynonymsGroupRow
                        groupId={item.SynonymsGroupId}
                        tags={item.SynonymsTags}   
                        index={index}
                        key={"synonymsItem-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this)}
                        Collapse={this.collapse.bind(this)}
                        onDelete={this.onDeleteSynonymsGroup.bind(this, item)}
                        id={id}>
                        <SynonymsGroupEditor
                            group={item}
                            Collapse={this.collapse.bind(this)}
                            onUpdate={this.onUpdateSynonymsGroup.bind(this)}
                            id={id}
                            openId={this.state.openId} />
                    </SynonymsGroupRow>
                );
            });
        }
    }

    render() {
        let opened = (this.state.openId === "add");
        return (
            <div>
                <div className="synonyms-group-items">
                    <div className="AddItemRow">
                        <div className="sectionTitle">{resx.get("Synonyms")}</div>
                        <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add")}>
                            <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                            </div> Add Group
                        </div>
                    </div>
                    <div className="synonyms-items-grid">
                        {this.renderHeader()}
                        <Collapse isOpened={opened} style={{ float: "left", width: "100%" }}>
                            <SynonymsGroupRow
                                tags={"-"}                                
                                index={"add"}
                                key={"aliasItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                onDelete={this.onDeleteSynonymsGroup.bind(this)}
                                id={"add"}>
                                <SynonymsGroupEditor
                                    Collapse={this.collapse.bind(this)}
                                    onUpdate={this.onUpdateSynonymsGroup.bind(this)}
                                    id={"add"}
                                    openId={this.state.openId} />
                            </SynonymsGroupRow>
                        </Collapse>
                        {this.renderedSynonymsGroups()}
                    </div>
                </div>

            </div >
        );
    }
}

SynonymsGroupsPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    synonymsGroups: PropTypes.array,    
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        synonymsGroups: state.search.synonymsGroups,        
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(SynonymsGroupsPanel);