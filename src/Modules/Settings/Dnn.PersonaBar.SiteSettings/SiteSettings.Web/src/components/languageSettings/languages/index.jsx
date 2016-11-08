import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    languages as LanguagesActions,
    visiblePanel as VisiblePanelActions,
    languageEditor as LanguageEditorActions
} from "../../../actions";
import LanguageRow from "./languageRow";
import LanguageEditor from "./languageEditor";
import Collapse from "react-collapse";
import Select from "dnn-select";
import "./style.less";
import { AddIcon } from "dnn-svg-icons";
import util from "../../../utils";
import resx from "../../../resources";

let tableFields = [];

class LanguagesPanel extends Component {
    constructor() {
        super();
        this.state = {
            languageList: [],
            openId: "",
            openMode: 1
        };
    }

    componentWillMount() {
        const {props} = this;

        if (props.languageList) {
            this.setState({
                languageList: props.languageList
            });
            return;
        }
        props.dispatch(LanguagesActions.getLanguages(props.portalId, props.cultureCode, (data) => {
            this.setState({
                languageList: Object.assign({}, data.Languages)
            });
        }));
    }

    componentWillReceiveProps(props) {
        let {state} = this;
        if (tableFields.length === 0) {
            tableFields.push({ "name": resx.get("Culture.Header"), "id": "Culture" });
            tableFields.push({ "name": resx.get("Enabled.Header"), "id": "Enabled" });
        }
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "languages-items header-" + field.id;
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

    toggle(openId, mode) {
        const {props, state} = this;
        if (props.languageClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(LanguagesActions.cancelLanguageClientModified());
                if (openId !== "") {
                    this.uncollapse(openId);
                    this.setState({
                        openMode: mode
                    });
                }
            });
        }
        else {
            if (openId !== "") {
                this.uncollapse(openId);
                this.setState({
                    openMode: mode
                });
            }
        }
    }

    onUpdateLanguage(languageDetail) {
        const {props, state} = this;
        if (languageDetail.LanguageId && languageDetail.LanguageId !== -1) {
            props.dispatch(LanguagesActions.updateLanguage(languageDetail, (data) => {
                util.utilities.notify(resx.get("LanguageUpdateSuccess"));
                this.collapse();
                props.dispatch(LanguagesActions.getLanguages(props.portalId, props.cultureCode));
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(LanguagesActions.addLanguage(languageDetail, (data) => {
                util.utilities.notify(resx.get("LanguageCreateSuccess"));
                this.collapse();
                props.dispatch(LanguagesActions.getLanguages(props.portalId, props.cultureCode));
                props.dispatch(LanguagesActions.getAllLanguages());
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onOpenEditor(language) {
        this.props.dispatch(VisiblePanelActions.selectPanel(3));
        this.props.dispatch(LanguageEditorActions.setLanguageBeingEdited(language));
    }

    /* eslint-disable react/no-danger */
    renderedLanguages() {
        let i = 0;
        if (this.props.languageList) {
            return this.props.languageList.map((item, index) => {
                let id = "row-" + i++;
                return (
                    <LanguageRow
                        languageId={item.LanguageId}
                        name={this.props.languageDisplayMode === "NATIVE" ? item.NativeName : item.EnglishName}
                        icon={item.Icon}
                        enabled={item.Enabled}
                        isDefault={item.IsDefault}
                        index={index}
                        key={"languageItem-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this)}
                        onOpenEditor={this.onOpenEditor.bind(this, item)}
                        Collapse={this.collapse.bind(this)}
                        id={id}>
                        <LanguageEditor
                            languageId={item.LanguageId}
                            languageDisplayMode={this.props.languageDisplayMode}
                            portalId={this.props.portalId}
                            Collapse={this.collapse.bind(this)}
                            onUpdate={this.onUpdateLanguage.bind(this)}
                            id={id}
                            openId={this.state.openId}
                            openMode={this.state.openMode}
                            />
                    </LanguageRow>
                );
            });
        }
    }

    render() {
        let opened = (this.state.openId === "add");
        return (
            <div>
                <div className="language-items">
                    <div className="AddItemRow">
                        <div className="sectionTitle-languages">{resx.get("Languages")}</div>
                        <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add", 1)}>
                            <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                            </div> Add Language
                        </div>
                    </div>
                    <div className="language-items-grid">
                        {this.renderHeader()}
                        <Collapse isOpened={opened} style={{ float: "left", width: "100%" }}>
                            <LanguageRow
                                name={"-"}
                                enabled={false}
                                isDefault={false}
                                index={"add"}
                                key={"languageItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                onOpenEditor={this.onOpenEditor.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"add"}>
                                <LanguageEditor
                                    portalId={this.props.portalId}
                                    languageDisplayMode={this.props.languageDisplayMode}
                                    Collapse={this.collapse.bind(this)}
                                    onUpdate={this.onUpdateLanguage.bind(this)}
                                    id={"add"}
                                    openId={this.state.openId}
                                    openMode={this.state.openMode}
                                    />
                            </LanguageRow>
                        </Collapse>
                        {this.renderedLanguages()}
                    </div>
                </div>

            </div >
        );
    }
}

LanguagesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    languageList: PropTypes.array,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string,
    languageDisplayMode: PropTypes.string,
    languageClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        languageList: state.languages.languageList,
        tabIndex: state.pagination.tabIndex,
        languageClientModified: state.languages.languageClientModified
    };
}

export default connect(mapStateToProps)(LanguagesPanel);