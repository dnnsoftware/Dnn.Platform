import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    search as SearchActions
} from "../../actions";
import SynonymsGroupRow from "./synonymsGroupRow";
import SynonymsGroupEditor from "./synonymsGroupEditor";
import { Dropdown, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";

let tableFields = [];

class SynonymsGroupsPanel extends Component {
    constructor() {
        super();
        this.state = {
            synonymsGroups: undefined,
            openId: "",
            culture: ""
        };
    }

    loadData() {
        const { props } = this;
        const culture = this.getCurrentCulture();
        props.dispatch(SearchActions.getSynonymsGroups(props.portalId, culture, (data) => {
            this.setState({
                synonymsGroups: Object.assign({}, data)
            });
        }));
    }

    componentDidMount() {
        const { props } = this;

        if (tableFields.length === 0) {
            tableFields.push({ "name": resx.get("SynonymsGroup.Header"), "id": "Synonyms" });
        }

        props.dispatch(SearchActions.getCultureList(props.portalId));

        this.loadData();
    }

    componentDidUpdate() {
        const { props } = this;

        if (props.synonymsGroups) {
            let portalIdChanged = false;
            let cultureCodeChanged = false;
            if (props.portalId === undefined || props.synonymsGroups.PortalId === props.portalId) {
                portalIdChanged = false;
            }
            else {
                portalIdChanged = true;
            }

            if (props.cultureCode === undefined || props.synonymsGroups.CultureCode === props.cultureCode) {
                cultureCodeChanged = false;
            }
            else {
                cultureCodeChanged = true;
            }

            if (portalIdChanged || cultureCodeChanged) {
                this.loadData();
            }
        }
    }

    getCurrentCulture() {
        const { state, props } = this;
        if(state.culture) {
            return state.culture;
        } else {
            if(props.synonymsGroups !== undefined && props.synonymsGroups.CultureCode !== undefined) {
                return props.synonymsGroups.CultureCode;
            } else {
                return props.cultureCode;
            }
        }
    }

    renderHeader() {
        if (this.props.synonymsGroups) {
            let tableHeaders = tableFields.map((field) => {
                let className = "synonyms-items header-" + field.id;
                return <div className={className} key={"header-" + field.id}>
                    <span>{field.name}&nbsp; </span>
                </div>;
            });

            if (this.props.synonymsGroups.SynonymsGroups.length > 0) {
                return <div className="header-row">{tableHeaders}</div>;                
            }
            else {
                return <div className="header-row-no-border">{tableHeaders}</div>;
            }
        }
    }

    unCollapse(id) {
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
            this.unCollapse(openId);
        }
    }

    onUpdateSynonymsGroup(group) {
        const {props} = this;
        const synonymsGroups = Object.assign({}, props.synonymsGroups);
        if (group.SynonymsGroupId) {
            synonymsGroups.SynonymsGroups = synonymsGroups.SynonymsGroups.map((item) => {
                if (item.SynonymsGroupId === group.SynonymsGroupId) {
                    return group;
                }
                else {
                    return item;
                }
            });

            props.dispatch(SearchActions.updateSynonymsGroup(Object.assign({PortalId: props.synonymsGroups.PortalId}, group), synonymsGroups, () => {
                util.utilities.notify(resx.get("SynonymsGroupUpdateSuccess"));
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(SearchActions.addSynonymsGroup(Object.assign({PortalId: props.synonymsGroups.PortalId}, group), props.synonymsGroups, () => {
                util.utilities.notify(resx.get("SynonymsGroupCreateSuccess"));
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onDeleteSynonymsGroup(group) {
        const {props} = this;
        util.utilities.confirm(resx.get("SynonymsGroupDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            const synonymsGroups = Object.assign({}, props.synonymsGroups);
            synonymsGroups.SynonymsGroups = synonymsGroups.SynonymsGroups.filter((item) => item.SynonymsGroupId !== group.SynonymsGroupId);
            props.dispatch(SearchActions.deleteSynonymsGroup(Object.assign({PortalId: props.synonymsGroups.PortalId}, group), synonymsGroups, () => {
                util.utilities.notify(resx.get("SynonymsGroupDeleteSuccess"));
                this.collapse();
            }, () => {
                util.utilities.notify(resx.get("SynonymsGroupDeleteError"));
            }));
        });
    }

    onSelectCulture(event) {
        let {props} = this;

        this.setState({
            culture: event.value
        });

        props.dispatch(SearchActions.getSynonymsGroups(props.portalId, event.value, (data) => {
            this.setState({
                synonymsGroups: Object.assign({}, data.SynonymsGroups)
            });
        }));
    }

    getCultureOptions() {
        const {props} = this;
        let options = [];
        if (props.cultures !== undefined) {
            options = props.cultures.map((item) => {
                return {
                    label: <div style={{ float: "left", display: "flex" }}>
                        <div className="language-flag">
                            <img src={item.Icon} alt={item.Name} />
                        </div>
                        <div className="language-name">{item.Name}</div>
                    </div>, value: item.Code
                };
            });
        }
        return options;
    }

    /* eslint-disable react/no-danger */
    renderedSynonymsGroups() {
        let i = 0;
        if (this.props.synonymsGroups) {
            return this.props.synonymsGroups.SynonymsGroups.map((item, index) => {
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
                        id={id}
                        visible={true}>
                        <SynonymsGroupEditor
                            group={item}
                            culture={this.state.culture}
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
                            <div className="add-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.AddIcon }}>
                            </div> {resx.get("cmdAddGroup")}
                        </div>
                        {this.props.cultures && this.props.cultures.length > 1 &&
                            <div className="synonyms-filter">
                                <Dropdown
                                    value={this.state.culture}
                                    style={{ width: "auto" }}
                                    options={this.getCultureOptions()}
                                    withBorder={false}
                                    onSelect={this.onSelectCulture.bind(this)}
                                />
                            </div>
                        }
                    </div>
                    <div className="synonyms-items-grid">
                        {this.renderHeader()}
                        <SynonymsGroupRow
                            tags={"-"}
                            index={"add"}
                            key={"aliasItem-add"}
                            closeOnClick={true}
                            openId={this.state.openId}
                            OpenCollapse={this.toggle.bind(this)}
                            Collapse={this.collapse.bind(this)}
                            onDelete={this.onDeleteSynonymsGroup.bind(this)}
                            id={"add"}
                            visible={opened}>
                            <SynonymsGroupEditor
                                Collapse={this.collapse.bind(this)}
                                culture={this.state.culture}
                                onUpdate={this.onUpdateSynonymsGroup.bind(this)}
                                id={"add"}
                                openId={this.state.openId} />
                        </SynonymsGroupRow>
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
    synonymsGroups: PropTypes.object,
    cultures: PropTypes.array,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        synonymsGroups: state.search.synonymsGroups,
        cultures: state.search.cultures,
        tabIndex: state.pagination.tabIndex,
        portalId: state.siteInfo ? state.siteInfo.portalId : undefined,
    };
}

export default connect(mapStateToProps)(SynonymsGroupsPanel);