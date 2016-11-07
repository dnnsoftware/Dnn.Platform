import React, {Component, PropTypes } from "react";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import InputGroup from "dnn-input-group";
import SingleLineInput from "dnn-single-line-input";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import localization from "../../../localization";
import styles from "./style.less";
import Tags from "dnn-tags";
import Label from "dnn-label";

class PageDetails extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    onChangeTags(tags) {
        const {onChangeField} = this.props;
        onChangeField("tags", tags.join(","));
    }

    onChangeUrl(event) {
        const {onChangeField} = this.props;
        let value = event.target.value;
        if (!value.startsWith("/")) {
            value = "/" + value;
        }  
        onChangeField("url", value);
    }

    render() {
        const {page, errors} = this.props;
        const tags = page.tags ? page.tags.split(",") : [];
        const domainName = window.location.host;

        return (
            <div className={styles.pageStandard}>
                <GridSystem>
                    <GridCell className="left-column">
                        <SingleLineInputWithError
                            label={localization.get("Name") + "*"}
                            tooltipMessage={localization.get("page_name_tooltip")}
                            error={!!errors.name}
                            errorMessage={errors.name}
                            value={page.name} 
                            onChange={this.onChangeField.bind(this, "name")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <SingleLineInputWithError
                            label={localization.get("Title")}
                            tooltipMessage={localization.get("page_title_tooltip")}
                            value={page.title}
                            onChange={this.onChangeField.bind(this, "title")} />
                    </GridCell>
                </GridSystem>
                <InputGroup>
                    <MultiLineInputWithError
                        label={localization.get("Description")}
                        value={page.description}
                        onChange={this.onChangeField.bind(this, "description")} />
                </InputGroup>
                <InputGroup>
                    <MultiLineInputWithError
                        label={localization.get("Keywords")}
                        value={page.keywords} 
                        onChange={this.onChangeField.bind(this, "keywords")} />
                </InputGroup>
                <GridSystem>
                    <GridCell className="left-column input-cell">
                        <Label label={localization.get("Tags")}/>
                        <Tags 
                            tags={tags} 
                            onUpdateTags={this.onChangeTags.bind(this)}/>
                    </GridCell>
                    <GridCell className="right-column input-cell">
                        <Label label={localization.get("URL")}/>
                        <InputGroup>
                            <span className="input-group-addon">{domainName}</span>
                            <SingleLineInput 
                                value={page.url}
                                onChange={this.onChangeUrl.bind(this)} />
                        </InputGroup>
                    </GridCell>
                </GridSystem>
                <div style={{clear: "both"}}></div>
            </div>
        );
    }

}

PageDetails.propTypes = {
    page: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageDetails;