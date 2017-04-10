import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import Label from "dnn-label";
import Switch from "dnn-switch";
import Dropdown from "dnn-dropdown";
import RadioButtons from "dnn-radio-buttons";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Localization from "../../localization";
import {pageActions as PageActions} from "../../actions";
import styles from "./style.less";

class More extends Component {
    
    componentWillMount() {
        this.props.onFetchCacheProviderList();
    }
    
    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    onCacheProviderSelected(option) {
        this.props.onChangeField("cacheProvider", option.value);
        if (!this.props.page.cacheProvider && option.value) {
            this.props.onChangeField("cacheIncludeExclude", true);
        }        
    }

    onChangeIncludeExclude(value) {
        const include = value === "true";
        this.props.onChangeField("cacheIncludeExclude", include);
    }

    render() {
        const {page, onChangeField, cacheProviderList} = this.props;
        const cacheProviderOptions = cacheProviderList && 
            [{value: null, label:Localization.get("None")},
                 ...cacheProviderList.map(x => ({value: x, label: x}))];
        const includeExcludeOptions = [
            { value: true, label: Localization.get("Include") }, 
            { value: false, label: Localization.get("Exclude") }];

        return (
            <div className={styles.moreContainer}>
                <div className="title">
                    {Localization.get("Security")}
                </div>
                <GridSystem>
                    <GridCell className="left-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={Localization.get("SecureConnection_tooltip")}
                            label={Localization.get("SecureConnection")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.isSecure}
                            onChange={onChangeField.bind(this, "isSecure")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={Localization.get("DisableLink_tooltip")}
                            label={Localization.get("DisableLink")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.disableLink}
                            onChange={onChangeField.bind(this, "disableLink")} />
                    </GridCell>
                </GridSystem>

                <div className="title sectionTitle">
                    {Localization.get("CacheSettings")}
                </div>

                <GridSystem>
                    <GridCell className="left-column">
                        <Label
                            labelType="block"
                            tooltipMessage={Localization.get("OutputCacheProvider_tooltip")}
                            label={Localization.get("OutputCacheProvider")}
                            />
                        {cacheProviderOptions && 
                            <Dropdown options={cacheProviderOptions}
                                value={page.cacheProvider} 
                                onSelect={this.onCacheProviderSelected.bind(this)} 
                                withBorder={true} />}

                        {page.cacheProvider &&        
                            <SingleLineInputWithError
                                label={Localization.get("CacheDuration")}
                                tooltipMessage={Localization.get("CacheDuration_tooltip")} 
                                value={page.cacheDuration}
                                onChange={this.onChangeField.bind(this, "cacheDuration")} />}
                    </GridCell>

                    {page.cacheProvider &&
                        <GridCell className="right-column">
                            <Label
                                labelType="inline"
                                tooltipMessage={Localization.get("IncludeExcludeParams_tooltip")}
                                label={Localization.get("IncludeExcludeParams")}/>
                            <RadioButtons 
                                options={includeExcludeOptions} 
                                value={page.cacheIncludeExclude}
                                onChange={this.onChangeIncludeExclude.bind(this)}/>

                            {!page.cacheIncludeExclude &&
                                <SingleLineInputWithError
                                    label={Localization.get("IncludeParamsInCacheValidation")}
                                    tooltipMessage={Localization.get("IncludeParamsInCacheValidation_tooltip")} 
                                    value={page.cacheIncludeVaryBy} 
                                    onChange={this.onChangeField.bind(this, "cacheIncludeVaryBy")} />}

                            {page.cacheIncludeExclude &&
                                <SingleLineInputWithError
                                    label={Localization.get("ExcludeParamsInCacheValidation")}
                                    tooltipMessage={Localization.get("ExcludeParamsInCacheValidation_tooltip")} 
                                    value={page.cacheExcludeVaryBy} 
                                    onChange={this.onChangeField.bind(this, "cacheExcludeVaryBy")}/> }

                            <SingleLineInputWithError                                 
                                label={Localization.get("VaryByLimit")}
                                tooltipMessage={Localization.get("VaryByLimit_tooltip")}
                                value={page.cacheMaxVaryByCount}
                                onChange={this.onChangeField.bind(this, "cacheMaxVaryByCount")} />
                            
                        </GridCell>       
                    } 
                </GridSystem>
            </div>
        );
    }
}

More.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    cacheProviderList: PropTypes.array,
    onFetchCacheProviderList: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        cacheProviderList: state.pages.cacheProviderList
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators ({
        onFetchCacheProviderList: PageActions.fetchCacheProviderList
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(More);