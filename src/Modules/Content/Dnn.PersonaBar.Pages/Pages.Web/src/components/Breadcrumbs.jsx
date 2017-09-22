import React, {PropTypes} from "react";

const maxItems = 4;

const Breadcrumbs = ({items, onSelectedItem}) => {

    function onClick(index) {
        if (index < items.length - 1) {
            onSelectedItem({...items[index]});
        }
    }

    return (
        <div className="breadcrumbs-container">
            { items.length > maxItems && 
                <span className="more"
                    title={items.map(i => i.name).join(" > ")}
                    onClick={() => onClick(0)} /> }

            { items.slice(Math.max(items.length - maxItems, 0)).map((item, i) =>{

                return (
                <div key={item.id} onClick={() => onClick(item.tabId)}>
                    <span>{item.name}</span>
                </div>); } )
            }
        </div>
    );
};

Breadcrumbs.propTypes = {
    items: PropTypes.array.isRequired,
    onSelectedItem: PropTypes.func.isRequired
};

export default Breadcrumbs;