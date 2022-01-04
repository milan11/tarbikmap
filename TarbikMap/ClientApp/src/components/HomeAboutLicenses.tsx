import React, { Component } from "react";

type Props = {};
type State = { items: Item[]; selectedItem: number };

type Item = {
  name: string;
  author: string | null;
  repository: string | null;
  license: string | null;
  licenseText: string | null;
};

export class HomeAboutLicenses extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { items: [], selectedItem: -1 };
  }

  componentDidMount() {
    this.fetchLicenses();
  }

  async fetchLicenses() {
    const response = await fetch("/licenses.json");
    const json = await response.json();

    let items: Item[] = [];
    items = items.concat(json);

    items.sort((a, b) => {
      if (a.name < b.name) {
        return -1;
      }
      if (a.name > b.name) {
        return 1;
      }
      return 0;
    });

    this.setState({
      items: items,
    });
  }

  render() {
    return (
      <div className="collection">
        {this.state.items.map((item, i) => {
          if (i === this.state.selectedItem) {
            return this.renderDetails(item, i);
          } else {
            return this.renderNameOnly(item, i);
          }
        })}
      </div>
    );
  }

  renderNameOnly(item: Item, key: number) {
    return (
      // eslint-disable-next-line
      <a key={key} className="collection-item" onClick={() => this.setState({ selectedItem: this.state.selectedItem !== key ? key : -1 })} style={{ cursor: "pointer" }}>
        {item.name}
      </a>
    );
  }

  renderDetails(item: Item, key: number) {
    return (
      <div key={key} className="collection-item" onClick={() => this.setState({ selectedItem: this.state.selectedItem !== key ? key : -1 })} style={{ overflowY: "scroll" }}>
        {item.name ? (
          <div>
            <strong>Library:</strong> {item.name}
          </div>
        ) : null}
        {item.author ? (
          <div>
            <strong>Author:</strong> {item.author}
          </div>
        ) : null}
        {item.repository ? (
          <div>
            <strong>Repository:</strong> {item.repository}
          </div>
        ) : null}
        {item.license ? (
          <div>
            <strong>License:</strong> {item.license}
          </div>
        ) : null}
        {item.licenseText ? (
          <div>
            <strong>License text:</strong> <pre>{item.licenseText}</pre>
          </div>
        ) : null}
      </div>
    );
  }
}
