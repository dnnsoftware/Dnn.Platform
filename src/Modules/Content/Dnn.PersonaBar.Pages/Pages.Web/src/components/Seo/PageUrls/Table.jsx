import React, {Component, PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import Localization from "../../../localization";
import UrlRow from "./UrlRow";

class Table extends Component {
    onEdit(url) {
        
    }
    
    onDelete(url) {
        
    }
    
    getUrlRows(pageUrls) {
        if (!pageUrls || pageUrls.length === 0) {
            return null;
        }
        
        const {siteAliases, primaryAliasId, onChange} = this.props;
        return pageUrls.map(url => {
            return <UrlRow url={url}
                        isEditingModule={false}
                        siteAliases={siteAliases}
                        primaryAliasId={primaryAliasId}
                        onChange={onChange}
                        onEdit={this.onEdit.bind(this)}
                        onDelete={this.onDelete.bind(this)} />;
        });
    }
    
    render() {
        const {pageUrls, onEdit, onDelete} = this.props;
        const urlRows = this.getUrlRows(pageUrls);
        
        return (
            <div className="url-table">
                <div className="header-row">
                    <GridCell columnSize={50} >
                        {Localization.get("Url")}
                    </GridCell>
                    <GridCell  columnSize={20} >
                        {Localization.get("UrlType")}
                    </GridCell>
                    <GridCell  columnSize={30} >
                        {Localization.get("GeneratedBy")}
                    </GridCell>
                </div>
                {urlRows}
            </div>
        );
    }
}

Table.propTypes = {
    pageUrls: PropTypes.arrayOf(PropTypes.object),
    siteAliases: PropTypes.arrayOf(PropTypes.object).isRequired,
    primaryAliasId: PropTypes.number,
    onEdit: PropTypes.func,
    onChange: PropTypes.func,
    onDelete: PropTypes.func
};

export default Table;