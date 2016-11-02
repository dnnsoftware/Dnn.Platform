import React, {Component, PropTypes } from "react";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import InputGroup from "dnn-input-group";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Localization from "../../../localization";
import styles from "./style.less";

class PageDetails extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    render() {
        const {page} = this.props;

        return (
            <div className={styles.pageDetails}>
                <GridSystem>
                    <GridCell className="left-column">
                        <SingleLineInputWithError
                            label={Localization.get("Name")}
                            tooltipMessage={Localization.get("page_name_tooltip")}
                            value={page.name} 
                            onChange={this.onChangeField.bind(this, "name")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <SingleLineInputWithError
                            label={Localization.get("Title")}
                            tooltipMessage={Localization.get("page_title_tooltip")}
                            value={page.title}
                            onChange={this.onChangeField.bind(this, "title")} />
                    </GridCell>
                </GridSystem>
                <InputGroup>
                    <MultiLineInputWithError
                        label={Localization.get("Description")}
                        value={page.description}
                        onChange={this.onChangeField.bind(this, "description")} />
                </InputGroup>
                <InputGroup>
                    <MultiLineInputWithError
                        label={Localization.get("Keywords")}
                        value={page.keywords} 
                        onChange={this.onChangeField.bind(this, "keywords")} />
                </InputGroup>
                <GridSystem>
                    <GridCell className="left-column">
                        <SingleLineInputWithError
                            label={Localization.get("Tags")}
                            value={page.tags} 
                            onChange={this.onChangeField.bind(this, "tags")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <SingleLineInputWithError
                            label={Localization.get("URL")}
                            value={page.url}
                            enabled={false} />
                    </GridCell>
                </GridSystem>
            </div>
        );
    }

}

PageDetails.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageDetails;