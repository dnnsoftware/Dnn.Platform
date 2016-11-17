import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import Button from "dnn-button";
import Localization from "../../localization";
import Scrollbars from "react-custom-scrollbars";
import PageLanguage from "./PageLanguage";
import NotifyModal from "./NotifyModal";
import utils from "../../utils";
import style from "./style.less";

import LanguagesActions from "../../actions/languagesActions";

class PageLocalization extends Component {

    constructor() {
        super();
        this.state = {
            Locales: [],
            Modules: [],
            Pages: [],
            ErrorExists: false,
            showNotifyModal: false,
            notifyMessage: ""
        };
    }

    componentWillMount() {
        this.getLanguages();
    }

    getLanguages() {
        const {props, state} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.getLanguages(tabId, (data) => {
            this.setState(data);
        }));

    }

    onCloseNotifyModal() {
        this.setState({ showNotifyModal: false });
    }

    onOpenNotifyModal() {
        this.setState({ showNotifyModal: true });
    }

    makePageTranslatable() {
        const {props, state} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.makePageTranslatable(tabId, (data) => {
            this.getLanguages();
        }));
    }

    makePageNeutral() {
        const {props, state} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.makePageNeutral(tabId, (data) => {
            this.getLanguages();
        }));
    }

    onAddMissingLanguages (){
        const {props, state} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.addMissingLanguages(tabId, (data) => {
            this.getLanguages();
        }));
    }

    onNotifyTranslators (){}
    onUpdateLocalization (){}

    componentWillReceiveProps(newProps) {
    }

    renderPageLanguage(local, modules, page) {
        const Modules = modules && modules.Modules ? modules.Modules : [];
        return <PageLanguage
            local={local}
            modules={Modules}
            page={page}
            />;
    }

    getAllLanguages() {
        const {Locales, Modules, Pages} = this.state;
        const pageLanguages = Locales.map((l, index) => {
            if (!index) {
                return false;
            }
            return this.renderPageLanguage(Locales[index], Modules[index], Pages[index]);
        });
        return pageLanguages;
    }

    onSendNotifyMessage () {
        const {props, state} = this;
        const {tabId} = props.page;
        const params = {TabId: tabId, Text: this.state.notifyMessage};
        props.dispatch(LanguagesActions.notifyTranslators(params, (data) => {
            this.onCloseNotifyModal();
        }));
    }

    onUpdateNotifyMessage (e) {
        this.setState({notifyMessage: e.target.value});
    }

    render() {
        const {Locales, Modules, Pages, ErrorExists} = this.state;
        const Languages = this.getAllLanguages();
        const containerStyle = { width: (Languages.length - 1) * 250 };

        if (ErrorExists) {
            return <div className="neutral-page">
                <div className="left-panel">
                    <p>{Localization.get("NeutralPageText") }</p>
                    <p>{Localization.get("NeutralPageClickButton") }</p>
                </div>
                <div className="right-panel">
                    <Button
                        type="secondary"
                        onClick={this.makePageTranslatable.bind(this) }>
                        {Localization.get("MakePagesTranslatable") }
                    </Button>
                </div>
            </div>;
        }

        return <div className="page-localization">
            <div className="page-localization-container">
                <div className="default-language-container">
                    {this.renderPageLanguage(Locales[0], Modules[0], Pages[0]) }
                </div>
                <div className="languages-container">
                    <Scrollbars className="scrollArea content-vertical"
                        autoHeight
                        autoHeightMin={0}
                        autoHeightMax={600}>
                        <div style={containerStyle}>
                            {Languages}
                        </div>
                    </Scrollbars>
                </div>
            </div>
            <div className="button-container">
                <Button
                    type="secondary"
                    className="float-left"
                    onClick={this.makePageNeutral.bind(this) }>
                    {Localization.get("MakePageNeutral") }
                </Button>
                <Button
                    type="secondary"
                    className="float-left"
                    onClick={this.onAddMissingLanguages.bind(this) }>
                    {Localization.get("AddMissingLanguages") }
                </Button>
                <Button
                    type="primary"
                    className="float-right"
                    onClick={this.onUpdateLocalization.bind(this) }>
                    {Localization.get("UpdateLocalization") }
                </Button>
                <Button
                    type="secondary"
                    className="float-right"
                    onClick={this.onOpenNotifyModal.bind(this) }>
                    {Localization.get("NotifyTranslators") }
                </Button>
            </div>
            {this.state.showNotifyModal && <NotifyModal 
                onSend={this.onSendNotifyMessage.bind(this)}
                onUpdateMessage={this.onUpdateNotifyMessage.bind(this)}
                notifyMessage={this.state.notifyMessage}
                onClose={this.onCloseNotifyModal.bind(this)} />}
        </div>;
    }
}

PageLocalization.propTypes = {
    page: PropTypes.object.isRequired
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes,
        defaultPortalThemeName: state.theme.defaultPortalThemeName,
        defaultPortalLayout: state.theme.defaultPortalLayout,
        defaultPortalContainer: state.theme.defaultPortalContainer,
        layouts: state.theme.layouts,
        containers: state.theme.containers
    };
}

export default connect(mapStateToProps)(PageLocalization);
