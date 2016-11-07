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
import localization from "../../localization";
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
    }

    onChangeIncludeExclude(value) {
        const include = value === "true";
        this.props.onChangeField("cacheIncludeExclude", include);
    }

    render() {
        const {page, onChangeField, cacheProviderList} = this.props;
        const cacheProviderOptions = cacheProviderList && 
            [{value: null, label:localization.get("None")},
                 ...cacheProviderList.map(x => ({value: x, label: x}))];
        const includeExcludeOptions = [{ value: true, label: "Include" }, { value: false, label: "Exclude" }];

        return (
            <div className={styles.moreContainer}>
                <div className="title">
                    {localization.get("Security")}
                </div>
                <GridSystem>
                    <GridCell className="left-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("secure_connection_tooltip")}
                            label={localization.get("Secure Connection")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.isSecure}
                            onChange={onChangeField.bind(this, "isSecure")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={localization.get("allow_indexing_tooltip")}
                            label={localization.get("Allow Indexing")}
                            />
                        <Switch
                            labelHidden={true}
                            value={page.allowIndex}
                            onChange={onChangeField.bind(this, "allowIndex")} />
                    </GridCell>
                </GridSystem>

                <div className="title sectionTitle">
                    {localization.get("Cache settings")}
                </div>

                <GridSystem>
                    <GridCell className="left-column">
                        <Label
                            labelType="block"
                            tooltipMessage={localization.get("output_cache_provider_tooltip")}
                            label={localization.get("Output Cache Provider")}
                            />
                        {cacheProviderOptions && 
                            <Dropdown options={cacheProviderOptions}
                                value={page.cacheProvider} 
                                onSelect={this.onCacheProviderSelected.bind(this)} 
                                withBorder={true} />}

                        {page.cacheProvider &&        
                            <SingleLineInputWithError
                                label={localization.get("Cache Duration (seconds)")}
                                tooltipMessage={localization.get("cache_duration_tooltip")} 
                                value={page.cacheDuration}
                                onChange={this.onChangeField.bind(this, "cacheDuration")} />}
                    </GridCell>

                    {page.cacheProvider &&
                        <GridCell className="right-column">
                            <Label
                                labelType="inline"
                                tooltipMessage={localization.get("include_exclude_params_by_default_tooltip")}
                                label={localization.get("Include / Exclude Params by default")}/>
                            <RadioButtons 
                                options={includeExcludeOptions} 
                                value={page.cacheIncludeExclude}
                                onChange={this.onChangeIncludeExclude.bind(this)}/>

                            {!page.cacheIncludeExclude &&
                                <SingleLineInputWithError
                                    label={localization.get("Include Params In Cache validation")}
                                    tooltipMessage={localization.get("include_params_in_cache_validation_tooltip")} 
                                    value={page.cacheIncludeVaryBy} />}

                            {page.cacheIncludeExclude &&
                                <SingleLineInputWithError
                                    label={localization.get("Exclude Params In Cache validation")}
                                    tooltipMessage={localization.get("exclude_params_in_cache_validation_tooltip")} 
                                    value={page.cacheExcludeVaryBy} /> }

                            <SingleLineInputWithError                                 
                                label={localization.get("Vary By Limit")}
                                tooltipMessage={localization.get("vary_by_limit_tooltip")}
                                value={page.cacheMaxVaryByCount} />
                            
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